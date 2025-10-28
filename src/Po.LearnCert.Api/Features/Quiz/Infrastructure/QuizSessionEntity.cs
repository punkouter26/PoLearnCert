using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Quiz session entity for Azure Table Storage.
/// PartitionKey = UserId
/// RowKey = SessionId
/// </summary>
public class QuizSessionEntity : TableEntityBase
{
    /// <summary>
    /// Session ID (same as RowKey).
    /// </summary>
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// User ID (same as PartitionKey).
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Certification ID.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Subtopic ID (null = all subtopics).
    /// </summary>
    public string? SubtopicId { get; set; }

    /// <summary>
    /// Comma-separated list of question IDs.
    /// </summary>
    public string QuestionIds { get; set; } = default!;

    /// <summary>
    /// Current question index (0-based).
    /// </summary>
    public int CurrentQuestionIndex { get; set; }

    /// <summary>
    /// Number of correct answers.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Number of incorrect answers.
    /// </summary>
    public int IncorrectAnswers { get; set; }

    /// <summary>
    /// Session start time.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// Session completion time.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Whether session is completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}
