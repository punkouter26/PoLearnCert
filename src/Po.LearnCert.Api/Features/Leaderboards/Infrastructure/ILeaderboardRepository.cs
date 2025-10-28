namespace Po.LearnCert.Api.Features.Leaderboards.Infrastructure;

/// <summary>
/// Repository interface for leaderboard data access.
/// </summary>
public interface ILeaderboardRepository
{
    /// <summary>
    /// Gets leaderboard entries for a specific certification and time period.
    /// </summary>
    /// <param name="certificationId">The certification identifier.</param>
    /// <param name="timePeriod">The time period (AllTime, Monthly, Weekly).</param>
    /// <param name="skip">Number of entries to skip (for pagination).</param>
    /// <param name="take">Number of entries to take (for pagination).</param>
    /// <returns>List of leaderboard entries ordered by best score descending.</returns>
    Task<List<LeaderboardEntity>> GetLeaderboardAsync(
        string certificationId,
        string timePeriod,
        int skip = 0,
        int take = 50);

    /// <summary>
    /// Gets the total count of entries for a specific certification and time period.
    /// </summary>
    Task<int> GetTotalEntriesAsync(string certificationId, string timePeriod);

    /// <summary>
    /// Gets a specific user's leaderboard entry.
    /// </summary>
    Task<LeaderboardEntity?> GetUserEntryAsync(
        string certificationId,
        string timePeriod,
        string userId);

    /// <summary>
    /// Updates or creates a leaderboard entry for a user.
    /// </summary>
    /// <param name="entity">The leaderboard entity to upsert.</param>
    Task UpsertAsync(LeaderboardEntity entity);

    /// <summary>
    /// Recalculates ranks for all entries in a leaderboard.
    /// </summary>
    Task RecalculateRanksAsync(string certificationId, string timePeriod);
}
