using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Certifications.Services;

/// <summary>
/// Service for managing certifications and subtopics.
/// </summary>
public interface ICertificationService
{
    /// <summary>
    /// Gets all available certifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of certifications.</returns>
    Task<List<CertificationDto>> GetAllCertificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific certification by ID.
    /// </summary>
    /// <param name="certificationId">The certification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The certification details.</returns>
    Task<CertificationDto?> GetCertificationByIdAsync(string certificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all subtopics for a certification.
    /// </summary>
    /// <param name="certificationId">The certification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of subtopics.</returns>
    Task<List<SubtopicDto>> GetSubtopicsAsync(string certificationId, CancellationToken cancellationToken = default);
}
