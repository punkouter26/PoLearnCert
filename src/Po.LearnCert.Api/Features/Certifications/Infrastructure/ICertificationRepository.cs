namespace Po.LearnCert.Api.Features.Certifications.Infrastructure;

/// <summary>
/// Repository interface for certification operations.
/// </summary>
public interface ICertificationRepository
{
    /// <summary>
    /// Gets all certifications.
    /// </summary>
    Task<IEnumerable<CertificationEntity>> GetAllCertificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a certification by ID.
    /// </summary>
    Task<CertificationEntity?> GetCertificationByIdAsync(string certificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all subtopics for a certification.
    /// </summary>
    Task<IEnumerable<SubtopicEntity>> GetSubtopicsByCertificationAsync(string certificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific subtopic.
    /// </summary>
    Task<SubtopicEntity?> GetSubtopicByIdAsync(string certificationId, string subtopicId, CancellationToken cancellationToken = default);
}
