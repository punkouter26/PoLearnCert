namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Detailed statistics for a user's session history.
/// </summary>
public class SessionHistoryDto
{
    /// <summary>
    /// User ID.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// List of quiz sessions ordered by date (most recent first).
    /// </summary>
    public List<SessionSummaryDto> Sessions { get; set; } = new();

    /// <summary>
    /// Total number of sessions.
    /// </summary>
    public int TotalSessions { get; set; }

    /// <summary>
    /// Date range filter applied (if any).
    /// </summary>
    public DateRange? DateRange { get; set; }
}

/// <summary>
/// Summary information for a single quiz session.
/// </summary>
public class SessionSummaryDto
{
    /// <summary>
    /// Session ID.
    /// </summary>
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// Certification ID.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Certification name.
    /// </summary>
    public string CertificationName { get; set; } = default!;

    /// <summary>
    /// Session start date and time.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Session completion date and time.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Total number of questions in the session.
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Number of questions answered.
    /// </summary>
    public int QuestionsAnswered { get; set; }

    /// <summary>
    /// Number of correct answers.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Session score percentage.
    /// </summary>
    public decimal ScorePercentage { get; set; }

    /// <summary>
    /// Total time spent on the session.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Session status.
    /// </summary>
    public SessionStatus Status { get; set; }
}

/// <summary>
/// Date range filter for session history.
/// </summary>
public class DateRange
{
    /// <summary>
    /// Start date (inclusive).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date (inclusive).
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Session status enumeration.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Session is in progress.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Session was completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Session was abandoned before completion.
    /// </summary>
    Abandoned = 2
}