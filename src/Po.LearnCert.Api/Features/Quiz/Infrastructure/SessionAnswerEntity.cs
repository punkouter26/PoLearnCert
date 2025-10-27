using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Session answer entity for Azure Table Storage.
/// PartitionKey = SessionId
/// RowKey = QuestionId
/// </summary>
public class SessionAnswerEntity : TableEntityBase
{
    /// <summary>
    /// Session ID (same as PartitionKey).
    /// </summary>
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// Question ID (same as RowKey).
    /// </summary>
    public string QuestionId { get; set; } = default!;

    /// <summary>
    /// Selected answer choice ID.
    /// </summary>
    public string SelectedChoiceId { get; set; } = default!;

    /// <summary>
    /// Whether the answer was correct.
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// When the answer was submitted.
    /// </summary>
    public DateTimeOffset AnsweredAt { get; set; }

    /// <summary>
    /// Time taken to answer in seconds.
    /// </summary>
    public int TimeSeconds { get; set; }
}
