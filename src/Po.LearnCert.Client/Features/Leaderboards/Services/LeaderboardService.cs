using System.Net.Http.Json;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Client.Features.Leaderboards.Services;

/// <summary>
/// HTTP client service for leaderboard operations.
/// </summary>
public class LeaderboardService : ILeaderboardService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(HttpClient httpClient, ILogger<LeaderboardService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LeaderboardDto?> GetLeaderboardAsync(
        string certificationId,
        string timePeriod = "AllTime",
        int skip = 0,
        int take = 50,
        string? userId = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"timePeriod={Uri.EscapeDataString(timePeriod)}",
                $"skip={skip}",
                $"take={take}"
            };

            if (!string.IsNullOrEmpty(userId))
            {
                queryParams.Add($"userId={Uri.EscapeDataString(userId)}");
            }

            var url = $"/api/leaderboards/{Uri.EscapeDataString(certificationId)}?{string.Join("&", queryParams)}";

            _logger.LogInformation("Fetching leaderboard from {Url}", url);

            var leaderboard = await _httpClient.GetFromJsonAsync<LeaderboardDto>(url);

            if (leaderboard != null)
            {
                _logger.LogInformation(
                    "Retrieved leaderboard for {CertId} {Period} with {Count} entries",
                    certificationId, timePeriod, leaderboard.Entries.Count);
            }

            return leaderboard;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching leaderboard for {CertId}", certificationId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leaderboard for {CertId}", certificationId);
            return null;
        }
    }
}
