using Azure;
using Azure.Data.Tables;
using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation for Azure Table Storage operations.
/// </summary>
/// <typeparam name="T">The entity type that inherits from TableEntityBase</typeparam>
public class TableRepository<T> : IRepository<T> where T : TableEntityBase, new()
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TableRepository<T>> _logger;

    public TableRepository(TableServiceClient tableServiceClient, ILogger<TableRepository<T>> logger)
    {
        _logger = logger;

        // Table name follows pattern: PoLearnCert[EntityName]
        var tableName = $"PoLearnCert{typeof(T).Name.Replace("Entity", "")}";
        _tableClient = tableServiceClient.GetTableClient(tableName);

        // Create table if it doesn't exist
        _tableClient.CreateIfNotExists();

        _logger.LogInformation("Initialized TableRepository for {TableName}", tableName);
    }

    public async Task<T?> GetByIdAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Entity not found: PartitionKey={PartitionKey}, RowKey={RowKey}", partitionKey, rowKey);
            return null;
        }
    }

    public async Task<IEnumerable<T>> GetByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        var filter = $"PartitionKey eq '{partitionKey}'";
        return await QueryAsync(filter, cancellationToken);
    }

    public async Task<IEnumerable<T>> QueryAsync(string filter, CancellationToken cancellationToken = default)
    {
        var entities = new List<T>();

        await foreach (var entity in _tableClient.QueryAsync<T>(filter, cancellationToken: cancellationToken))
        {
            entities.Add(entity);
        }

        _logger.LogDebug("Query returned {Count} entities with filter: {Filter}", entities.Count, filter);
        return entities;
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tableClient.AddEntityAsync(entity, cancellationToken);
            _logger.LogInformation("Created entity: PartitionKey={PartitionKey}, RowKey={RowKey}",
                entity.PartitionKey, entity.RowKey);
            return entity;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            _logger.LogError("Entity already exists: PartitionKey={PartitionKey}, RowKey={RowKey}",
                entity.PartitionKey, entity.RowKey);
            throw new InvalidOperationException("Entity already exists", ex);
        }
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace, cancellationToken);
            _logger.LogInformation("Updated entity: PartitionKey={PartitionKey}, RowKey={RowKey}",
                entity.PartitionKey, entity.RowKey);
            return entity;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError("Entity not found for update: PartitionKey={PartitionKey}, RowKey={RowKey}",
                entity.PartitionKey, entity.RowKey);
            throw new InvalidOperationException("Entity not found", ex);
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            _logger.LogError("Concurrency conflict: PartitionKey={PartitionKey}, RowKey={RowKey}",
                entity.PartitionKey, entity.RowKey);
            throw new InvalidOperationException("Concurrency conflict - entity was modified", ex);
        }
    }

    public async Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellationToken);
            _logger.LogInformation("Deleted entity: PartitionKey={PartitionKey}, RowKey={RowKey}",
                partitionKey, rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Attempted to delete non-existent entity: PartitionKey={PartitionKey}, RowKey={RowKey}",
                partitionKey, rowKey);
        }
    }

    public async Task<bool> ExistsAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tableClient.GetEntityAsync<T>(partitionKey, rowKey, select: new[] { "PartitionKey" }, cancellationToken: cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
