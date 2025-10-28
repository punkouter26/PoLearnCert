using Azure;
using Azure.Data.Tables;

namespace Po.LearnCert.Api.Infrastructure.Entities;

/// <summary>
/// Base class for all Azure Table Storage entities in the PoLearnCert platform.
/// Implements ITableEntity for Azure SDK compatibility.
/// </summary>
public abstract class TableEntityBase : ITableEntity
{
    /// <summary>
    /// Partition key for table storage partitioning.
    /// </summary>
    public string PartitionKey { get; set; } = default!;

    /// <summary>
    /// Row key for unique identification within a partition.
    /// </summary>
    public string RowKey { get; set; } = default!;

    /// <summary>
    /// Timestamp of the last modification (managed by Azure Table Storage).
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control (managed by Azure Table Storage).
    /// </summary>
    public ETag ETag { get; set; }
}
