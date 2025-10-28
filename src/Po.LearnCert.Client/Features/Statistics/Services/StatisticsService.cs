using Po.LearnCert.Shared.Models;
using System.Net.Http.Json;

namespace Po.LearnCert.Client.Features.Statistics.Services;

/// <summary>
/// HTTP client service for accessing user performance statistics from the API
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(HttpClient httpClient, ILogger<StatisticsService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<UserStatisticsDto?> GetUserStatisticsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        try
        {
            _logger.LogInformation("Fetching statistics for user {UserId}", userId);

            var response = await _httpClient.GetAsync($"/api/userstatistics/{userId}");
            response.EnsureSuccessStatusCode();

            var statistics = await response.Content.ReadFromJsonAsync<UserStatisticsDto>();

            _logger.LogInformation("Successfully retrieved statistics for user {UserId}", userId);
            return statistics;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching statistics for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching statistics for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<SubtopicPerformanceDto>> GetSubtopicPerformanceAsync(string userId, string? certificationId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        try
        {
            var url = $"/api/userstatistics/{userId}/subtopics";
            if (!string.IsNullOrWhiteSpace(certificationId))
            {
                url += $"?certificationId={certificationId}";
            }

            _logger.LogInformation("Fetching subtopic performance for user {UserId}, certification {CertificationId}",
                userId, certificationId ?? "all");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var performance = await response.Content.ReadFromJsonAsync<List<SubtopicPerformanceDto>>();

            _logger.LogInformation("Successfully retrieved {Count} subtopic performance records for user {UserId}",
                performance?.Count ?? 0, userId);

            return performance ?? new List<SubtopicPerformanceDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching subtopic performance for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching subtopic performance for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<SessionSummaryDto>> GetSessionHistoryAsync(string userId, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (limit < 1)
            throw new ArgumentException("Limit must be at least 1", nameof(limit));

        try
        {
            _logger.LogInformation("Fetching session history for user {UserId}, limit {Limit}", userId, limit);

            var response = await _httpClient.GetAsync($"/api/userstatistics/{userId}/sessions?limit={limit}");
            response.EnsureSuccessStatusCode();

            var sessionHistory = await response.Content.ReadFromJsonAsync<SessionHistoryDto>();

            _logger.LogInformation("Successfully retrieved {Count} session records for user {UserId}",
                sessionHistory?.Sessions?.Count ?? 0, userId);

            return sessionHistory?.Sessions ?? new List<SessionSummaryDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching session history for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching session history for user {UserId}", userId);
            throw;
        }
    }
}
