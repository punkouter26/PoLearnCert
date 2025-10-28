namespace Po.LearnCert.Shared.Models;

/// <summary>
/// User's overall performance statistics across certifications.
/// </summary>
public class UserStatisticsDto
{
    /// <summary>
    /// User ID.
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
    public decimal OverallAccuracy { get; set; }

    /// <summary>
    /// Average session score percentage.
    /// </summary>
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Best session score percentage.
    /// </summary>
    public decimal BestScore { get; set; }

    /// <summary>
    /// Most recent session date.
    /// </summary>
    public DateTime? LastSessionDate { get; set; }

    /// <summary>
    /// Total study time across all sessions.
    /// </summary>
    public TimeSpan TotalStudyTime { get; set; }

    /// <summary>
    /// Performance breakdown by certification.
    /// </summary>
    public List<CertificationPerformanceDto> CertificationPerformance { get; set; } = new();
}

/// <summary>
/// Performance statistics for a specific certification.
/// </summary>
public class CertificationPerformanceDto
{
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
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Best score for this certification.
    /// </summary>
    public decimal BestScore { get; set; }

    /// <summary>
    /// Total questions answered for this certification.
    /// </summary>
    public int QuestionsAnswered { get; set; }

    /// <summary>
    /// Accuracy percentage for this certification.
    /// </summary>
    public decimal Accuracy { get; set; }
}