using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Leaderboards.Infrastructure;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Leaderboards.Services;

/// <summary>
/// Service for leaderboard operations.
/// Implements FR-011 (leaderboard filtering), FR-012 (time periods), FR-013 (top performers).
/// </summary>
public class LeaderboardService
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly ICertificationRepository _certificationRepository;
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(
        ILeaderboardRepository leaderboardRepository,
        ICertificationRepository certificationRepository,
        ILogger<LeaderboardService> logger)
    {
        _leaderboardRepository = leaderboardRepository;
        _certificationRepository = certificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets leaderboard with filtering by certification and time period (FR-011, FR-012, FR-013).
    /// </summary>
    public async Task<LeaderboardDto> GetLeaderboardAsync(
        string certificationId,
        string timePeriod = "AllTime",
        int skip = 0,
        int take = 50,
        string? currentUserId = null)
    {
        _logger.LogInformation(
            "Getting leaderboard for {CertId} {Period} (skip: {Skip}, take: {Take})",
            certificationId, timePeriod, skip, take);

        // Validate time period
        if (!IsValidTimePeriod(timePeriod))
        {
            throw new ArgumentException($"Invalid time period: {timePeriod}. Must be AllTime, Monthly, or Weekly.");
        }

        // Get certification name
        var certification = await _certificationRepository.GetCertificationByIdAsync(certificationId);
        var certificationName = certification?.Name ?? certificationId;

        // Get leaderboard entries
        var entries = await _leaderboardRepository.GetLeaderboardAsync(
            certificationId, timePeriod, skip, take);

        // Get total count
        var totalEntries = await _leaderboardRepository.GetTotalEntriesAsync(
            certificationId, timePeriod);

        // Map to DTOs and mark current user
        var entryDtos = entries.Select(e => new LeaderboardEntryDto
        {
            UserId = e.UserId,
            Username = e.Username,
            Rank = e.Rank,
            BestScore = e.BestScore,
            QuizzesTaken = e.QuizzesTaken,
            AverageScore = e.AverageScore,
            LastAttemptDate = e.LastAttemptDate,
            IsCurrentUser = currentUserId != null && e.UserId == currentUserId
        }).ToList();

        return new LeaderboardDto
        {
            CertificationId = certificationId,
            CertificationName = certificationName,
            TimePeriod = timePeriod,
            TotalEntries = totalEntries,
            Entries = entryDtos,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates leaderboard entry after quiz completion.
    /// </summary>
    public async Task UpdateLeaderboardAsync(
        string userId,
        string username,
        string certificationId,
        int score,
        DateTime attemptDate)
    {
        _logger.LogInformation(
            "Updating leaderboard for user {UserId} on {CertId} with score {Score}",
            userId, certificationId, score);

        // Update all time periods
        var timePeriods = new[] { "AllTime", "Monthly", "Weekly" };

        foreach (var period in timePeriods)
        {
            // Check if we should update this time period
            if (!ShouldIncludeInTimePeriod(attemptDate, period))
            {
                _logger.LogDebug(
                    "Skipping {Period} update for attempt on {Date}",
                    period, attemptDate);
                continue;
            }

            // Get existing entry or create new
            var existing = await _leaderboardRepository.GetUserEntryAsync(
                certificationId, period, userId);

            if (existing == null)
            {
                // Create new entry
                var newEntry = new LeaderboardEntity
                {
                    PartitionKey = LeaderboardEntity.CreatePartitionKey(certificationId, period),
                    RowKey = LeaderboardEntity.CreateRowKey(userId),
                    UserId = userId,
                    Username = username,
                    BestScore = score,
                    QuizzesTaken = 1,
                    AverageScore = score,
                    LastAttemptDate = attemptDate
                };

                await _leaderboardRepository.UpsertAsync(newEntry);
            }
            else
            {
                // Update existing entry
                existing.BestScore = Math.Max(existing.BestScore, score);
                existing.QuizzesTaken++;
                existing.AverageScore = ((existing.AverageScore * (existing.QuizzesTaken - 1)) + score) / existing.QuizzesTaken;
                existing.LastAttemptDate = attemptDate;
                existing.Username = username; // Update in case it changed

                await _leaderboardRepository.UpsertAsync(existing);
            }

            // Recalculate ranks for this leaderboard
            await _leaderboardRepository.RecalculateRanksAsync(certificationId, period);
        }

        _logger.LogInformation(
            "Successfully updated leaderboards for user {UserId}",
            userId);
    }

    private static bool IsValidTimePeriod(string timePeriod)
    {
        return timePeriod is "AllTime" or "Monthly" or "Weekly";
    }

    private static bool ShouldIncludeInTimePeriod(DateTime attemptDate, string timePeriod)
    {
        var now = DateTime.UtcNow;

        return timePeriod switch
        {
            "AllTime" => true,
            "Monthly" => attemptDate >= now.AddMonths(-1),
            "Weekly" => attemptDate >= now.AddDays(-7),
            _ => false
        };
    }
}
