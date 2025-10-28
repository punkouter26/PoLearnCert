using Azure.Data.Tables;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;

namespace Po.LearnCert.Api.Repositories;

/// <summary>
/// Azure Table Storage implementation for subtopic repository.
/// </summary>
public class SubtopicRepository : ISubtopicRepository
{
    private readonly TableClient _tableClient;
    private const string TableName = "Subtopics";

    public SubtopicRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<SubtopicEntity?> GetByIdAsync(string subtopicId)
    {
        // We need to search across all partitions since we only have the subtopic ID
        await foreach (var entity in _tableClient.QueryAsync<SubtopicEntity>(
            filter: $"SubtopicId eq '{subtopicId}'"))
        {
            return entity;
        }

        return null;
    }

    public async Task<IEnumerable<SubtopicEntity>> GetByCertificationIdAsync(string certificationId)
    {
        var results = new List<SubtopicEntity>();

        await foreach (var entity in _tableClient.QueryAsync<SubtopicEntity>(
            filter: $"PartitionKey eq '{certificationId}'"))
        {
            results.Add(entity);
        }

        return results;
    }

    public async Task<IEnumerable<SubtopicEntity>> GetAllAsync()
    {
        var results = new List<SubtopicEntity>();

        await foreach (var entity in _tableClient.QueryAsync<SubtopicEntity>())
        {
            results.Add(entity);
        }

        return results;
    }
}