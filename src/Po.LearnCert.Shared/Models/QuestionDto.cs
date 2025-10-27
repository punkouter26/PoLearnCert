namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Quiz question DTO.
/// </summary>
public class QuestionDto
{
    /// <summary>
    /// Unique question identifier.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Subtopic this question belongs to.
    /// </summary>
    public string SubtopicId { get; set; } = default!;

    /// <summary>
    /// Question text.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Answer choices for this question.
    /// </summary>
    public List<AnswerChoiceDto> Choices { get; set; } = new();

    /// <summary>
    /// Explanation shown after answering.
    /// </summary>
    public string Explanation { get; set; } = default!;
}
