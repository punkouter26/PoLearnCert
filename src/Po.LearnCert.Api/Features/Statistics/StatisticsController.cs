using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Statistics;

/// <summary>
/// API endpoints for user statistics and performance tracking.
/// Consolidated controller supporting both path parameters and query parameters.
/// </summary>
[ApiController]
[Route("api")]
public class StatisticsController : ControllerBase
{
    private readonly IUserStatisticsService _statisticsService;

    public StatisticsController(IUserStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Get comprehensive statistics for a user via path parameter.
    /// </summary>
    [HttpGet("users/{userId}/statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        try
        {
            var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving user statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Get comprehensive statistics for a user via query parameter.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics([FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        try
        {
            var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving user statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Get subtopic performance breakdown for a user via path parameter.
    /// </summary>
    [HttpGet("users/{userId}/statistics/subtopics")]
    public async Task<ActionResult<IEnumerable<SubtopicPerformanceDto>>> GetUserSubtopicPerformance(
        string userId,
        [FromQuery] string? certificationId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        try
        {
            var performance = await _statisticsService.GetSubtopicPerformanceAsync(userId, certificationId);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving subtopic performance: {ex.Message}");
        }
    }

    /// <summary>
    /// Get subtopic performance breakdown for a user via query parameters.
    /// </summary>
    [HttpGet("statistics/subtopics")]
    public async Task<ActionResult<IEnumerable<SubtopicPerformanceDto>>> GetSubtopicPerformance(
        [FromQuery] string? userId,
        [FromQuery] string? certificationId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        try
        {
            var performance = await _statisticsService.GetSubtopicPerformanceAsync(userId, certificationId);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving subtopic performance: {ex.Message}");
        }
    }

    /// <summary>
    /// Get session history for a user.
    /// </summary>
    [HttpGet("users/{userId}/statistics/sessions")]
    public async Task<ActionResult<SessionHistoryDto>> GetSessionHistory(
        string userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? limit = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        if (limit.HasValue && limit.Value <= 0)
        {
            return BadRequest("Limit must be greater than 0.");
        }

        DateRange? dateRange = null;
        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate.Value > endDate.Value)
            {
                return BadRequest("Start date cannot be after end date.");
            }

            dateRange = new DateRange
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };
        }

        try
        {
            var history = await _statisticsService.GetSessionHistoryAsync(userId, dateRange, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving session history: {ex.Message}");
        }
    }
}
