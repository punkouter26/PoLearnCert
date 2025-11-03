using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Certifications.Infrastructure;

/// <summary>
/// Certification entity for Azure Table Storage.
/// PartitionKey = "CERT" (all certifications in same partition)
/// RowKey = CertificationId (e.g., "az-900", "sy0-701")
/// </summary>
public class CertificationEntity : TableEntityBase
{
    public CertificationEntity()
    {
        PartitionKey = "CERT";
    }

    /// <summary>
    /// Certification ID (same as RowKey).
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Legacy property for backward compatibility.
    /// </summary>
    public string CertificationId
    {
        get => Id;
        set => Id = value;
    }

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Icon emoji.
    /// </summary>
    public string Icon { get; set; } = "📚";

    /// <summary>
    /// Total number of questions available.
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Passing score percentage.
    /// </summary>
    public int PassingScore { get; set; } = 70;
}
