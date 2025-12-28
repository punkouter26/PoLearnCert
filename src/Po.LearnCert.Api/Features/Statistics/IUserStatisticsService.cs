using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Statistics;

/// <summary>
/// Service interface for user statistics operations.
/// </summary>
public interface IUserStatisticsService
{
    /// <summary>
    /// Get comprehensive statistics for a user.
    /// </summary>
    Task<UserStatisticsDto> GetUserStatisticsAsync(string userId);

    /// <summary>
    /// Get subtopic performance breakdown for a user.
    /// </summary>
    Task<IEnumerable<SubtopicPerformanceDto>> GetSubtopicPerformanceAsync(string userId, string? certificationId = null);

    /// <summary>
    /// Get session history for a user.
    /// </summary>
    Task<SessionHistoryDto> GetSessionHistoryAsync(string userId, DateRange? dateRange = null, int? limit = null);

    /// <summary>
    /// Update statistics after a quiz session completion.
    /// </summary>
    Task UpdateStatisticsAfterSessionAsync(string userId, string sessionId);

    /// <summary>
    /// Calculate performance level based on accuracy percentage.
    /// </summary>
    PerformanceLevel CalculatePerformanceLevel(decimal accuracy);
}
