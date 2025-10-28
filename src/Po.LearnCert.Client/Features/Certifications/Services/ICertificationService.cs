using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Client.Features.Certifications.Services;

/// <summary>
/// Service interface for certification operations.
/// </summary>
public interface ICertificationService
{
    /// <summary>
    /// Gets all available certifications.
    /// </summary>
    Task<List<CertificationDto>> GetAllCertificationsAsync();

    /// <summary>
    /// Gets a specific certification by ID.
    /// </summary>
    Task<CertificationDto?> GetCertificationByIdAsync(string certificationId);

    /// <summary>
    /// Gets all subtopics for a certification.
    /// </summary>
    Task<List<SubtopicDto>> GetSubtopicsAsync(string certificationId);
}
