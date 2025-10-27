using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Certifications.Services;

/// <summary>
/// Service for managing certifications and subtopics.
/// </summary>
public class CertificationService : ICertificationService
{
    private readonly ICertificationRepository _repository;
    private readonly ILogger<CertificationService> _logger;

    public CertificationService(
        ICertificationRepository repository,
        ILogger<CertificationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<CertificationDto>> GetAllCertificationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all certifications");

        var entities = await _repository.GetAllCertificationsAsync(cancellationToken);

        var dtos = new List<CertificationDto>();
        foreach (var entity in entities)
        {
            var subtopics = await _repository.GetSubtopicsByCertificationAsync(entity.RowKey, cancellationToken);

            dtos.Add(new CertificationDto
            {
                Id = entity.RowKey,
                Name = entity.Name,
                Description = entity.Description,
                TotalQuestions = entity.TotalQuestions,
                Subtopics = subtopics.Select(s => new SubtopicDto
                {
                    Id = s.RowKey,
                    CertificationId = s.CertificationId,
                    Name = s.Name,
                    QuestionCount = s.QuestionCount
                }).ToList()
            });
        }

        _logger.LogInformation("Retrieved {Count} certifications", dtos.Count);
        return dtos;
    }

    /// <inheritdoc/>
    public async Task<CertificationDto?> GetCertificationByIdAsync(
        string certificationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting certification {CertificationId}", certificationId);

        var entity = await _repository.GetCertificationByIdAsync(certificationId, cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("Certification {CertificationId} not found", certificationId);
            return null;
        }

        var subtopics = await _repository.GetSubtopicsByCertificationAsync(certificationId, cancellationToken);

        return new CertificationDto
        {
            Id = entity.RowKey,
            Name = entity.Name,
            Description = entity.Description,
            TotalQuestions = entity.TotalQuestions,
            Subtopics = subtopics.Select(s => new SubtopicDto
            {
                Id = s.RowKey,
                CertificationId = s.CertificationId,
                Name = s.Name,
                QuestionCount = s.QuestionCount
            }).ToList()
        };
    }

    /// <inheritdoc/>
    public async Task<List<SubtopicDto>> GetSubtopicsAsync(
        string certificationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting subtopics for certification {CertificationId}", certificationId);

        var entities = await _repository.GetSubtopicsByCertificationAsync(certificationId, cancellationToken);

        var dtos = entities.Select(e => new SubtopicDto
        {
            Id = e.RowKey,
            CertificationId = e.CertificationId,
            Name = e.Name,
            QuestionCount = e.QuestionCount
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} subtopics for certification {CertificationId}",
            dtos.Count, certificationId);

        return dtos;
    }
}
