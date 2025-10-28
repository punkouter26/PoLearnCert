using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Client.Features.Statistics.Services;

/// <summary>
/// Service interface for accessing user performance statistics
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Gets overall statistics for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>User statistics DTO</returns>
    Task<UserStatisticsDto?> GetUserStatisticsAsync(string userId);

    /// <summary>
    /// Gets subtopic performance breakdown for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="certificationId">Optional certification filter</param>
    /// <returns>List of subtopic performance DTOs</returns>
    Task<List<SubtopicPerformanceDto>> GetSubtopicPerformanceAsync(string userId, string? certificationId = null);

    /// <summary>
    /// Gets session history for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="limit">Maximum number of sessions to return</param>
    /// <returns>List of performance record DTOs</returns>
    Task<List<SessionSummaryDto>> GetSessionHistoryAsync(string userId, int limit = 10);
}
