namespace Po.LearnCert.Shared.Models;

/// <summary>
/// Certification information DTO.
/// </summary>
public class CertificationDto
{
    /// <summary>
    /// Unique certification identifier (e.g., "AZ900", "SECPLUS").
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Display name (e.g., "Microsoft Azure Fundamentals AZ-900").
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Short description of the certification.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Total number of questions available for this certification.
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// List of subtopics within this certification.
    /// </summary>
    public List<SubtopicDto> Subtopics { get; set; } = new();
}
