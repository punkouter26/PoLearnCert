using System.Net.Http.Json;

namespace Po.LearnCert.Client.Features.QuestionGeneration;

/// <summary>
/// HTTP client service for question generation API.
/// </summary>
public class QuestionGenerationService : IQuestionGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuestionGenerationService> _logger;

    public QuestionGenerationService(HttpClient httpClient, ILogger<QuestionGenerationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GenerationResultDto> GenerateForSubtopicAsync(
        string certificationId,
        string subtopicId,
        string subtopicName,
        int count,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating {Count} questions for subtopic {SubtopicId}", count, subtopicId);

        var request = new GenerateForSubtopicRequest(certificationId, subtopicId, subtopicName, count);
        var response = await _httpClient.PostAsJsonAsync("api/questions/generate/subtopic", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerationResultDto>(cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to parse generation result");
    }

    public async Task<GenerationResultDto> GenerateForCertificationAsync(
        string certificationId,
        int questionsPerSubtopic,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating {Count} questions per subtopic for certification {CertificationId}",
            questionsPerSubtopic, certificationId);

        var request = new GenerateForCertificationRequest(certificationId, questionsPerSubtopic);
        var response = await _httpClient.PostAsJsonAsync("api/questions/generate/certification", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerationResultDto>(cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to parse generation result");
    }
}
