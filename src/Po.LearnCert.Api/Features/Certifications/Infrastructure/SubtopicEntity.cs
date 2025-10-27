using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Certifications.Infrastructure;

/// <summary>
/// Subtopic entity for Azure Table Storage.
/// PartitionKey = CertificationId
/// RowKey = SubtopicId
/// </summary>
public class SubtopicEntity : TableEntityBase
{
    /// <summary>
    /// Subtopic ID (same as RowKey).
    /// </summary>
    public string SubtopicId { get; set; } = default!;

    /// <summary>
    /// Parent certification ID (same as PartitionKey).
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Subtopic name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Number of questions in this subtopic.
    /// </summary>
    public int QuestionCount { get; set; }
}
