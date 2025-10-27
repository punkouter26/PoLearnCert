namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Quiz session results and final score.
/// </summary>
public class QuizResultDto
{
    /// <summary>
    /// Session ID.
    /// </summary>
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// Certification name.
    /// </summary>
    public string CertificationName { get; set; } = default!;

    /// <summary>
    /// Subtopic name (if filtered).
    /// </summary>
    public string? SubtopicName { get; set; }

    /// <summary>
    /// Number of correct answers.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Number of incorrect answers.
    /// </summary>
    public int IncorrectAnswers { get; set; }

    /// <summary>
    /// Total questions in session.
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Score percentage (0-100).
    /// </summary>
    public decimal ScorePercentage { get; set; }

    /// <summary>
    /// Time taken to complete (in seconds).
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// Session start time.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// Session completion time.
    /// </summary>
    public DateTimeOffset CompletedAt { get; set; }
}
