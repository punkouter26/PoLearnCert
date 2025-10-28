using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Client.Features.Leaderboards.Services;

/// <summary>
/// Service interface for leaderboard operations.
/// </summary>
public interface ILeaderboardService
{
    /// <summary>
    /// Gets leaderboard for a specific certification.
    /// </summary>
    /// <param name="certificationId">Certification identifier</param>
    /// <param name="timePeriod">Time period filter (AllTime, Monthly, Weekly)</param>
    /// <param name="skip">Number of entries to skip for pagination</param>
    /// <param name="take">Number of entries to take for pagination</param>
    /// <param name="userId">Optional current user ID to highlight their entry</param>
    Task<LeaderboardDto?> GetLeaderboardAsync(
        string certificationId,
        string timePeriod = "AllTime",
        int skip = 0,
        int take = 50,
        string? userId = null);
}
