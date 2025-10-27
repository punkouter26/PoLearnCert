using System.ComponentModel.DataAnnotations;

namespace Po.LearnCert.Shared.Contracts;

/// <summary>
/// Request to create a new quiz session.
/// </summary>
public class CreateQuizSessionRequest
{
    /// <summary>
    /// Certification ID (required).
    /// </summary>
    [Required]
    public string CertificationId { get; set; } = default!;

    /// <summary>
    /// Subtopic ID (optional - null means all subtopics).
    /// </summary>
    public string? SubtopicId { get; set; }

    /// <summary>
    /// Number of questions (default: 20, min: 1, max: 50).
    /// </summary>
    [Range(1, 50)]
    public int QuestionCount { get; set; } = 20;
}
