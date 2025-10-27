using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Question entity for Azure Table Storage.
/// PartitionKey = CertificationId
/// RowKey = QuestionId
/// </summary>
public class QuestionEntity : TableEntityBase
{
    /// <summary>
    /// Question ID (same as RowKey).
    /// </summary>
    public string QuestionId { get; set; } = default!;

    /// <summary>
    /// Subtopic ID for filtering questions.
    /// </summary>
    public string SubtopicId { get; set; } = default!;

    /// <summary>
    /// Certification ID (same as PartitionKey).
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Question text.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Explanation of the correct answer.
    /// </summary>
    public string Explanation { get; set; } = default!;

    /// <summary>
    /// Difficulty level of the question.
    /// </summary>
    public string DifficultyLevel { get; set; } = default!;

    /// <summary>
    /// Date when the question was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
