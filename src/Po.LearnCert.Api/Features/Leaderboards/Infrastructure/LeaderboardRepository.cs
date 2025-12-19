using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Leaderboards.Infrastructure;

/// <summary>
/// Repository for leaderboard data access using Azure Table Storage.
/// </summary>
public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly TableClient _tableClient;
    private readonly ILogger<LeaderboardRepository> _logger;

    public LeaderboardRepository(
        TableServiceClient tableServiceClient,
        ILogger<LeaderboardRepository> logger)
    {
        _logger = logger;
        _tableClient = tableServiceClient.GetTableClient("PoLearnCertLeaderboards");
        _tableClient.CreateIfNotExists();
    }

    public async Task<List<LeaderboardEntity>> GetLeaderboardAsync(
        string certificationId,
        string timePeriod,
        int skip = 0,
        int take = 50)
    {
        var partitionKey = LeaderboardEntity.CreatePartitionKey(certificationId, timePeriod);

        var query = _tableClient.QueryAsync<LeaderboardEntity>(
            filter: $"PartitionKey eq '{partitionKey}'");

        var entities = new List<LeaderboardEntity>();
        await foreach (var entity in query)
        {
            entities.Add(entity);
        }

        // Sort by BestScore descending, then by LastAttemptDate ascending (earlier is better for ties)
        var sorted = entities
            .OrderByDescending(e => e.BestScore)
            .ThenBy(e => e.LastAttemptDate)
            .Skip(skip)
            .Take(take)
            .ToList();

        _logger.LogInformation(
            "Retrieved {Count} leaderboard entries for {CertId} {Period}",
            sorted.Count, certificationId, timePeriod);

        return sorted;
    }

    public async Task<int> GetTotalEntriesAsync(string certificationId, string timePeriod)
    {
        var partitionKey = LeaderboardEntity.CreatePartitionKey(certificationId, timePeriod);

        var query = _tableClient.QueryAsync<LeaderboardEntity>(
            filter: $"PartitionKey eq '{partitionKey}'",
            select: new[] { "PartitionKey" });

        var count = 0;
        await foreach (var _ in query)
        {
            count++;
        }

        return count;
    }

    public async Task<LeaderboardEntity?> GetUserEntryAsync(
        string certificationId,
        string timePeriod,
        string userId)
    {
        var partitionKey = LeaderboardEntity.CreatePartitionKey(certificationId, timePeriod);
        var rowKey = LeaderboardEntity.CreateRowKey(userId);

        try
        {
            var response = await _tableClient.GetEntityAsync<LeaderboardEntity>(partitionKey, rowKey);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Leaderboard entry not found for user {UserId}", userId);
            return null;
        }
    }

    public async Task UpsertAsync(LeaderboardEntity entity)
    {
        entity.LastUpdated = DateTime.UtcNow;
        await _tableClient.UpsertEntityAsync(entity);

        _logger.LogInformation(
            "Upserted leaderboard entry for user {UserId} in {PartitionKey}",
            entity.UserId, entity.PartitionKey);
    }

    public async Task RecalculateRanksAsync(string certificationId, string timePeriod)
    {
        var partitionKey = LeaderboardEntity.CreatePartitionKey(certificationId, timePeriod);

        var query = _tableClient.QueryAsync<LeaderboardEntity>(
            filter: $"PartitionKey eq '{partitionKey}'");

        var entities = new List<LeaderboardEntity>();
        await foreach (var entity in query)
        {
            entities.Add(entity);
        }

        // Sort and assign ranks
        var sorted = entities
            .OrderByDescending(e => e.BestScore)
            .ThenBy(e => e.LastAttemptDate)
            .ToList();

        var rank = 1;
        foreach (var entity in sorted)
        {
            entity.Rank = rank++;
            await _tableClient.UpdateEntityAsync(entity, entity.ETag);
        }

        _logger.LogInformation(
            "Recalculated ranks for {Count} entries in {PartitionKey}",
            entities.Count, partitionKey);
    }
}
