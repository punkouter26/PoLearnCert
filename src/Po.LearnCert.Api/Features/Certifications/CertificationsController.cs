using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Features.Certifications.Services;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Certifications;

/// <summary>
/// Controller for certification and subtopic operations.
/// </summary>
[ApiController]
[Route("api/certifications")]
public class CertificationsController : ControllerBase
{
    private readonly ICertificationService _certificationService;
    private readonly ILogger<CertificationsController> _logger;

    public CertificationsController(
        ICertificationService certificationService,
        ILogger<CertificationsController> logger)
    {
        _certificationService = certificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available certifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of certifications with their subtopics.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CertificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CertificationDto>>> GetAllCertifications(
        CancellationToken cancellationToken)
    {
        var certifications = await _certificationService.GetAllCertificationsAsync(cancellationToken);
        return Ok(certifications);
    }

    /// <summary>
    /// Gets a specific certification by ID.
    /// </summary>
    /// <param name="id">The certification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The certification details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CertificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificationDto>> GetCertification(
        string id,
        CancellationToken cancellationToken)
    {
        var certification = await _certificationService.GetCertificationByIdAsync(id, cancellationToken);

        if (certification == null)
        {
            return NotFound(new { message = $"Certification {id} not found." });
        }

        return Ok(certification);
    }

    /// <summary>
    /// Gets all subtopics for a specific certification.
    /// </summary>
    /// <param name="id">The certification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of subtopics.</returns>
    [HttpGet("{id}/subtopics")]
    [ProducesResponseType(typeof(List<SubtopicDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SubtopicDto>>> GetSubtopics(
        string id,
        CancellationToken cancellationToken)
    {
        var subtopics = await _certificationService.GetSubtopicsAsync(id, cancellationToken);
        return Ok(subtopics);
    }
}
