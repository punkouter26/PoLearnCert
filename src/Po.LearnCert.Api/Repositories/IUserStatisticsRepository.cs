using Po.LearnCert.Api.Entities;

namespace Po.LearnCert.Api.Repositories;

/// <summary>
/// Repository interface for user statistics operations.
/// </summary>
public interface IUserStatisticsRepository
{
    /// <summary>
    /// Get overall statistics for a user.
    /// </summary>
    Task<UserStatisticsEntity?> GetUserStatisticsAsync(string userId);

    /// <summary>
    /// Update overall statistics for a user.
    /// </summary>
    Task<UserStatisticsEntity> UpdateUserStatisticsAsync(UserStatisticsEntity entity);

    /// <summary>
    /// Get certification performance for a user.
    /// </summary>
    Task<IEnumerable<CertificationPerformanceEntity>> GetCertificationPerformanceAsync(string userId);

    /// <summary>
    /// Get certification performance for a specific certification.
    /// </summary>
    Task<CertificationPerformanceEntity?> GetCertificationPerformanceAsync(string userId, string certificationId);

    /// <summary>
    /// Update certification performance for a user.
    /// </summary>
    Task<CertificationPerformanceEntity> UpdateCertificationPerformanceAsync(CertificationPerformanceEntity entity);

    /// <summary>
    /// Get subtopic performance for a user.
    /// </summary>
    Task<IEnumerable<SubtopicPerformanceEntity>> GetSubtopicPerformanceAsync(string userId);

    /// <summary>
    /// Get subtopic performance for a specific certification.
    /// </summary>
    Task<IEnumerable<SubtopicPerformanceEntity>> GetSubtopicPerformanceAsync(string userId, string certificationId);

    /// <summary>
    /// Get subtopic performance for a specific subtopic.
    /// </summary>
    Task<SubtopicPerformanceEntity?> GetSingleSubtopicPerformanceAsync(string userId, string subtopicId);

    /// <summary>
    /// Update subtopic performance for a user.
    /// </summary>
    Task<SubtopicPerformanceEntity> UpdateSubtopicPerformanceAsync(SubtopicPerformanceEntity entity);

    /// <summary>
    /// Delete all statistics for a user.
    /// </summary>
    Task DeleteUserStatisticsAsync(string userId);
}