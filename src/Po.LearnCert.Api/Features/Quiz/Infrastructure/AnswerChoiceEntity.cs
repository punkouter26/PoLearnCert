using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Answer choice entity for Azure Table Storage.
/// PartitionKey = QuestionId
/// RowKey = ChoiceId
/// </summary>
public class AnswerChoiceEntity : TableEntityBase
{
    /// <summary>
    /// Choice ID (same as RowKey).
    /// </summary>
    public string ChoiceId { get; set; } = default!;

    /// <summary>
    /// Question ID (same as PartitionKey).
    /// </summary>
    public string QuestionId { get; set; } = default!;

    /// <summary>
    /// Choice text.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Whether this is the correct answer.
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Display order (A, B, C, D).
    /// </summary>
    public int Order { get; set; }
}
