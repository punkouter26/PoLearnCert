namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Subtopic within a certification.
/// </summary>
public class SubtopicDto
{
    /// <summary>
    /// Unique subtopic identifier.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Parent certification ID.
    /// </summary>
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Subtopic name (e.g., "Cloud Concepts", "Identity and Access").
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Number of questions available in this subtopic.
    /// </summary>
    public int QuestionCount { get; set; }
}
