using Po.LearnCert.Shared.Contracts;
using Po.LearnCert.Shared.Models;
using System.Net.Http.Json;

namespace Po.LearnCert.Client.Features.Quiz.Services;

/// <summary>
/// HTTP client service for quiz session operations.
/// </summary>
public class QuizSessionService : IQuizSessionService
{
    private readonly HttpClient _httpClient;

    public QuizSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<QuizSessionDto> CreateSessionAsync(CreateQuizSessionRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/quiz/sessions", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QuizSessionDto>()
            ?? throw new InvalidOperationException("Failed to deserialize quiz session response.");
    }

    /// <inheritdoc/>
    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/quiz/sessions/{request.SessionId}/answers", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SubmitAnswerResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize submit answer response.");
    }

    /// <inheritdoc/>
    public async Task<QuizSessionDto> GetSessionAsync(string sessionId)
    {
        var response = await _httpClient.GetAsync($"api/quiz/sessions/{sessionId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QuizSessionDto>()
            ?? throw new InvalidOperationException("Failed to deserialize quiz session response.");
    }

    /// <inheritdoc/>
    public async Task<QuizResultDto> GetSessionResultsAsync(string sessionId)
    {
        var response = await _httpClient.GetAsync($"api/quiz/sessions/{sessionId}/results");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QuizResultDto>()
            ?? throw new InvalidOperationException("Failed to deserialize quiz results response.");
    }

    /// <inheritdoc/>
    public async Task<QuestionDto> GetQuestionAsync(string certificationId, string questionId)
    {
        var response = await _httpClient.GetAsync($"api/questions/{certificationId}/{questionId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QuestionDto>()
            ?? throw new InvalidOperationException("Failed to deserialize question response.");
    }
}
