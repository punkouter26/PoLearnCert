using Azure.Data.Tables;

namespace Po.LearnCert.Api.Entities;

/// <summary>
/// Entity for tracking user performance by certification.
/// Partition Key: UserId
/// Row Key: "CERT_{CertificationId}"
/// </summary>
public class CertificationPerformanceEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!; // UserId
    public string RowKey { get; set; } = default!; // "CERT_{CertificationId}"
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }

    /// <summary>
    /// User ID (same as PartitionKey).
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Certification ID.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Certification name.
    /// </summary>
    public string CertificationName { get; set; } = default!;

    /// <summary>
    /// Number of sessions for this certification.
    /// </summary>
    public int SessionCount { get; set; }

    /// <summary>
    /// Average score for this certification.
    /// </summary>
    public double AverageScore { get; set; }

    /// <summary>
    /// Best score for this certification.
    /// </summary>
    public double BestScore { get; set; }

    /// <summary>
    /// Total questions answered for this certification.
    /// </summary>
    public int QuestionsAnswered { get; set; }

    /// <summary>
    /// Total correct answers for this certification.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Accuracy percentage for this certification.
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// Date when performance was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Create entity from user and certification information.
    /// </summary>
    public static CertificationPerformanceEntity Create(string userId, string certificationId, string certificationName)
    {
        return new CertificationPerformanceEntity
        {
            PartitionKey = userId,
            RowKey = $"CERT_{certificationId}",
            UserId = userId,
            CertificationId = certificationId,
            CertificationName = certificationName,
            LastUpdated = DateTime.UtcNow
        };
    }
}