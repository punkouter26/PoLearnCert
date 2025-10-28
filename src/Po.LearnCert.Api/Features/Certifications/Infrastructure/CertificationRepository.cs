using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Certifications.Infrastructure;

/// <summary>
/// Repository for certification operations.
/// </summary>
public class CertificationRepository : ICertificationRepository
{
    private readonly TableClient _certificationsTable;
    private readonly TableClient _subtopicsTable;
    private readonly ILogger<CertificationRepository> _logger;

    public CertificationRepository(TableServiceClient tableServiceClient, ILogger<CertificationRepository> logger)
    {
        _logger = logger;
        _certificationsTable = tableServiceClient.GetTableClient("PoLearnCertCertifications");
        _subtopicsTable = tableServiceClient.GetTableClient("PoLearnCertSubtopics");

        _certificationsTable.CreateIfNotExists();
        _subtopicsTable.CreateIfNotExists();
    }

    public async Task<IEnumerable<CertificationEntity>> GetAllCertificationsAsync(CancellationToken cancellationToken = default)
    {
        var certifications = new List<CertificationEntity>();

        await foreach (var cert in _certificationsTable.QueryAsync<CertificationEntity>(cancellationToken: cancellationToken))
        {
            certifications.Add(cert);
        }

        _logger.LogDebug("Retrieved {Count} certifications", certifications.Count);
        return certifications;
    }

    public async Task<CertificationEntity?> GetCertificationByIdAsync(string certificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _certificationsTable.GetEntityAsync<CertificationEntity>("CERT", certificationId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Certification not found: {CertificationId}", certificationId);
            return null;
        }
    }

    public async Task<IEnumerable<SubtopicEntity>> GetSubtopicsByCertificationAsync(string certificationId, CancellationToken cancellationToken = default)
    {
        var subtopics = new List<SubtopicEntity>();
        var filter = $"PartitionKey eq '{certificationId}'";

        await foreach (var subtopic in _subtopicsTable.QueryAsync<SubtopicEntity>(filter, cancellationToken: cancellationToken))
        {
            subtopics.Add(subtopic);
        }

        _logger.LogDebug("Retrieved {Count} subtopics for certification {CertificationId}", subtopics.Count, certificationId);
        return subtopics;
    }

    public async Task<SubtopicEntity?> GetSubtopicByIdAsync(string certificationId, string subtopicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _subtopicsTable.GetEntityAsync<SubtopicEntity>(certificationId, subtopicId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Subtopic not found: {CertificationId}/{SubtopicId}", certificationId, subtopicId);
            return null;
        }
    }
}
