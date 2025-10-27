namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Quiz session information.
/// </summary>
public class QuizSessionDto
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// User ID who owns this session.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Certification ID for this session.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Subtopic ID (optional - null means all subtopics).
    /// </summary>
    public string? SubtopicId { get; set; }

    /// <summary>
    /// List of question IDs in this session (20 questions).
    /// </summary>
    public List<string> QuestionIds { get; set; } = new();

    /// <summary>
    /// Current question index (0-based).
    /// </summary>
    public int CurrentQuestionIndex { get; set; }

    /// <summary>
    /// Number of correct answers so far.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Number of incorrect answers so far.
    /// </summary>
    public int IncorrectAnswers { get; set; }

    /// <summary>
    /// Session start time.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// Session completion time (null if not finished).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Whether the session is completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}
