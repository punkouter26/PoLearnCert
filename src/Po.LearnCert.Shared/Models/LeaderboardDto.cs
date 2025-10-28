namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Represents a leaderboard for a specific certification and time period.
/// </summary>
public class LeaderboardDto
{
    /// <summary>
    /// The certification identifier.
    /// </summary>
    public string CertificationId { get; set; } = string.Empty;

    /// <summary>
    /// The certification name.
    /// </summary>
    public string CertificationName { get; set; } = string.Empty;

    /// <summary>
    /// The time period for this leaderboard (AllTime, Monthly, Weekly).
    /// </summary>
    public string TimePeriod { get; set; } = "AllTime";

    /// <summary>
    /// The total number of entries in the full leaderboard (before pagination).
    /// </summary>
    public int TotalEntries { get; set; }

    /// <summary>
    /// The leaderboard entries for the current page.
    /// </summary>
    public List<LeaderboardEntryDto> Entries { get; set; } = new();

    /// <summary>
    /// The timestamp when this leaderboard was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
