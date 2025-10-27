using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Question entity for Azure Table Storage.
/// PartitionKey = SubtopicId
/// RowKey = QuestionId
/// </summary>
public class QuestionEntity : TableEntityBase
{
    /// <summary>
    /// Question ID (same as RowKey).
    /// </summary>
    public string QuestionId { get; set; } = default!;

    /// <summary>
    /// Subtopic ID (same as PartitionKey).
    /// </summary>
    public string SubtopicId { get; set; } = default!;

    /// <summary>
    /// Question text.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Explanation text shown after answering.
    /// </summary>
    public string Explanation { get; set; } = default!;

    /// <summary>
    /// Certification ID for easier querying.
    /// </summary>
    public string CertificationId { get; set; } = default!;
}
