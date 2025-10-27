using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Certifications.Infrastructure;

/// <summary>
/// Certification entity for Azure Table Storage.
/// PartitionKey = "CERT" (all certifications in same partition)
/// RowKey = CertificationId (e.g., "AZ900", "SECPLUS")
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
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Total number of questions available.
    /// </summary>
    public int TotalQuestions { get; set; }
}
