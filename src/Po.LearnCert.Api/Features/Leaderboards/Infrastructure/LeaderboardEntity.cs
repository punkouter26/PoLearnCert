using Azure;
using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Leaderboards.Infrastructure;

/// <summary>
/// Table entity for leaderboard entries.
/// PartitionKey: {CertificationId}_{TimePeriod} (e.g., "AZ-900_AllTime", "SecurityPlus_Monthly")
/// RowKey: UserId
/// </summary>
public class LeaderboardEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // User information
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    // Score information
    public int BestScore { get; set; }
    public int QuizzesTaken { get; set; }
    public double AverageScore { get; set; }

    // Metadata
    public DateTime LastAttemptDate { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Computed fields (for sorting)
    public int Rank { get; set; }

    /// <summary>
    /// Creates a partition key from certification ID and time period.
    /// </summary>
    public static string CreatePartitionKey(string certificationId, string timePeriod)
    {
        return $"{certificationId}_{timePeriod}";
    }

    /// <summary>
    /// Creates a row key from user ID.
    /// </summary>
    public static string CreateRowKey(string userId)
    {
        return userId;
    }
}
