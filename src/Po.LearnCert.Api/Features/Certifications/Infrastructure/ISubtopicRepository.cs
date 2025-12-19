namespace Po.LearnCert.Api.Features.Certifications.Infrastructure;

/// <summary>
/// Repository interface for subtopic operations.
/// </summary>
public interface ISubtopicRepository
{
    /// <summary>
    /// Get a subtopic by ID.
    /// </summary>
    Task<SubtopicEntity?> GetByIdAsync(string subtopicId);

    /// <summary>
    /// Get all subtopics for a certification.
    /// </summary>
    Task<IEnumerable<SubtopicEntity>> GetByCertificationIdAsync(string certificationId);

    /// <summary>
    /// Get all subtopics.
    /// </summary>
    Task<IEnumerable<SubtopicEntity>> GetAllAsync();
}
