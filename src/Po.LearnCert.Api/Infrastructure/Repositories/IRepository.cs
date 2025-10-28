namespace Po.LearnCert.Api.Infrastructure.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations on Azure Table Storage entities.
/// </summary>
/// <typeparam name="T">The entity type that implements ITableEntity</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its partition key and row key.
    /// </summary>
    /// <param name="partitionKey">The partition key</param>
    /// <param name="rowKey">The row key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> GetByIdAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities in a partition.
    /// </summary>
    /// <param name="partitionKey">The partition key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities in the partition</returns>
    Task<IEnumerable<T>> GetByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries entities using an OData filter expression.
    /// </summary>
    /// <param name="filter">OData filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities matching the filter</returns>
    Task<IEnumerable<T>> QueryAsync(string filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity in the table.
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created entity</returns>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the table.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the table.
    /// </summary>
    /// <param name="partitionKey">The partition key</param>
    /// <param name="rowKey">The row key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists in the table.
    /// </summary>
    /// <param name="partitionKey">The partition key</param>
    /// <param name="rowKey">The row key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity exists, otherwise false</returns>
    Task<bool> ExistsAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);
}
