using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Statistics.Entities;

/// <summary>
/// Entity for tracking user performance by subtopic.
/// Partition Key: UserId
/// Row Key: "SUBTOPIC_{SubtopicId}"
/// </summary>
public class SubtopicPerformanceEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!; // UserId
    public string RowKey { get; set; } = default!; // "SUBTOPIC_{SubtopicId}"
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }

    /// <summary>
    /// User ID (same as PartitionKey).
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Subtopic ID.
    /// </summary>
    public string SubtopicId { get; set; } = default!;

    /// <summary>
    /// Subtopic name.
    /// </summary>
    public string SubtopicName { get; set; } = default!;

    /// <summary>
    /// Certification ID this subtopic belongs to.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Certification name.
    /// </summary>
    public string CertificationName { get; set; } = default!;

    /// <summary>
    /// Number of questions answered for this subtopic.
    /// </summary>
    public int QuestionsAnswered { get; set; }

    /// <summary>
    /// Number of correct answers for this subtopic.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Accuracy percentage for this subtopic.
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// Performance level based on accuracy (0-4).
    /// </summary>
    public int PerformanceLevel { get; set; }

    /// <summary>
    /// Number of times this subtopic has been practiced.
    /// </summary>
    public int PracticeCount { get; set; }

    /// <summary>
    /// Date when this subtopic was last practiced (ISO 8601 string).
    /// </summary>
    public string? LastPracticedDate { get; set; }

    /// <summary>
    /// Average time spent per question in seconds.
    /// </summary>
    public long AverageTimePerQuestionSeconds { get; set; }

    /// <summary>
    /// Date when performance was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Create entity from user and subtopic information.
    /// </summary>
    public static SubtopicPerformanceEntity Create(string userId, string subtopicId, string subtopicName,
        string certificationId, string certificationName)
    {
        return new SubtopicPerformanceEntity
        {
            PartitionKey = userId,
            RowKey = $"SUBTOPIC_{subtopicId}",
            UserId = userId,
            SubtopicId = subtopicId,
            SubtopicName = subtopicName,
            CertificationId = certificationId,
            CertificationName = certificationName,
            LastUpdated = DateTime.UtcNow
        };
    }
}
