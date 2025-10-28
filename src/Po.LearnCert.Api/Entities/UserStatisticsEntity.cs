using Azure.Data.Tables;

namespace Po.LearnCert.Api.Entities;

/// <summary>
/// Entity for tracking user's overall performance statistics.
/// Partition Key: UserId
/// Row Key: "OVERALL"
/// </summary>
public class UserStatisticsEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!; // UserId
    public string RowKey { get; set; } = "OVERALL"; // Always "OVERALL"
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }

    /// <summary>
    /// User ID (same as PartitionKey).
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Total number of quiz sessions completed.
    /// </summary>
    public int TotalSessions { get; set; }

    /// <summary>
    /// Total questions answered across all sessions.
    /// </summary>
    public int TotalQuestionsAnswered { get; set; }

    /// <summary>
    /// Total correct answers across all sessions.
    /// </summary>
    public int TotalCorrectAnswers { get; set; }

    /// <summary>
    /// Overall accuracy percentage.
    /// </summary>
    public double OverallAccuracy { get; set; }

    /// <summary>
    /// Average session score percentage.
    /// </summary>
    public double AverageScore { get; set; }

    /// <summary>
    /// Best session score percentage.
    /// </summary>
    public double BestScore { get; set; }

    /// <summary>
    /// Most recent session date (ISO 8601 string).
    /// </summary>
    public string? LastSessionDate { get; set; }

    /// <summary>
    /// Total study time in minutes.
    /// </summary>
    public long TotalStudyTimeMinutes { get; set; }

    /// <summary>
    /// Date when statistics were last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Create entity from UserId.
    /// </summary>
    public static UserStatisticsEntity Create(string userId)
    {
        return new UserStatisticsEntity
        {
            PartitionKey = userId,
            RowKey = "OVERALL",
            UserId = userId,
            LastUpdated = DateTime.UtcNow
        };
    }
}