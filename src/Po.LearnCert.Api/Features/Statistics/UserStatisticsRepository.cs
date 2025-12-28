using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Statistics;

/// <summary>
/// Azure Table Storage implementation for user statistics repository.
/// </summary>
public class UserStatisticsRepository : IUserStatisticsRepository
{
    private readonly TableClient _tableClient;
    private const string TableName = "UserStatistics";

    public UserStatisticsRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<UserStatisticsEntity?> GetUserStatisticsAsync(string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<UserStatisticsEntity>(userId, "OVERALL");
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<UserStatisticsEntity> UpdateUserStatisticsAsync(UserStatisticsEntity entity)
    {
        entity.LastUpdated = DateTime.UtcNow;
        await _tableClient.UpsertEntityAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<CertificationPerformanceEntity>> GetCertificationPerformanceAsync(string userId)
    {
        var results = new List<CertificationPerformanceEntity>();

        await foreach (var entity in _tableClient.QueryAsync<CertificationPerformanceEntity>(
            filter: $"PartitionKey eq '{userId}' and RowKey ge 'CERT_' and RowKey lt 'CERT`'"))
        {
            results.Add(entity);
        }

        return results;
    }

    public async Task<CertificationPerformanceEntity?> GetCertificationPerformanceAsync(string userId, string certificationId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<CertificationPerformanceEntity>(userId, $"CERT_{certificationId}");
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<CertificationPerformanceEntity> UpdateCertificationPerformanceAsync(CertificationPerformanceEntity entity)
    {
        entity.LastUpdated = DateTime.UtcNow;
        await _tableClient.UpsertEntityAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<SubtopicPerformanceEntity>> GetSubtopicPerformanceAsync(string userId)
    {
        var results = new List<SubtopicPerformanceEntity>();

        await foreach (var entity in _tableClient.QueryAsync<SubtopicPerformanceEntity>(
            filter: $"PartitionKey eq '{userId}' and RowKey ge 'SUBTOPIC_' and RowKey lt 'SUBTOPIC`'"))
        {
            results.Add(entity);
        }

        return results;
    }

    public async Task<IEnumerable<SubtopicPerformanceEntity>> GetSubtopicPerformanceAsync(string userId, string certificationId)
    {
        var results = new List<SubtopicPerformanceEntity>();

        await foreach (var entity in _tableClient.QueryAsync<SubtopicPerformanceEntity>(
            filter: $"PartitionKey eq '{userId}' and RowKey ge 'SUBTOPIC_' and RowKey lt 'SUBTOPIC`' and CertificationId eq '{certificationId}'"))
        {
            results.Add(entity);
        }

        return results;
    }

    public async Task<SubtopicPerformanceEntity?> GetSingleSubtopicPerformanceAsync(string userId, string subtopicId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<SubtopicPerformanceEntity>(userId, $"SUBTOPIC_{subtopicId}");
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<SubtopicPerformanceEntity> UpdateSubtopicPerformanceAsync(SubtopicPerformanceEntity entity)
    {
        entity.LastUpdated = DateTime.UtcNow;
        await _tableClient.UpsertEntityAsync(entity);
        return entity;
    }

    public async Task DeleteUserStatisticsAsync(string userId)
    {
        var entitiesToDelete = new List<ITableEntity>();

        // Get all entities for the user
        await foreach (var entity in _tableClient.QueryAsync<TableEntity>(
            filter: $"PartitionKey eq '{userId}'"))
        {
            entitiesToDelete.Add(entity);
        }

        // Delete in batches
        var batches = entitiesToDelete
            .Select((entity, index) => new { entity, index })
            .GroupBy(x => x.index / 100) // Azure Table batch limit is 100
            .Select(g => g.Select(x => x.entity));

        foreach (var batch in batches)
        {
            foreach (var entity in batch)
            {
                await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
            }
        }
    }
}
