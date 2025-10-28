namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Represents a single entry in a leaderboard.
/// </summary>
public class LeaderboardEntryDto
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The user's display name or username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The user's rank position on the leaderboard (1-based).
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// The user's best score for this certification and time period.
    /// </summary>
    public int BestScore { get; set; }

    /// <summary>
    /// The total number of quiz sessions completed.
    /// </summary>
    public int QuizzesTaken { get; set; }

    /// <summary>
    /// The average score across all quiz sessions.
    /// </summary>
    public double AverageScore { get; set; }

    /// <summary>
    /// The timestamp of the last quiz session.
    /// </summary>
    public DateTime LastAttemptDate { get; set; }

    /// <summary>
    /// Whether this entry represents the current user.
    /// </summary>
    public bool IsCurrentUser { get; set; }
}
