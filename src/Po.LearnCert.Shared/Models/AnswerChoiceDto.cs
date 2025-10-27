namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Answer choice for a quiz question.
/// </summary>
public class AnswerChoiceDto
{
    /// <summary>
    /// Unique choice identifier.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Question this choice belongs to.
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
}
