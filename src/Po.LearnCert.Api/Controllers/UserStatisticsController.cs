using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Services;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Controllers;

/// <summary>
/// API endpoints for user statistics and performance tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserStatisticsController : ControllerBase
{
    private readonly IUserStatisticsService _statisticsService;

    public UserStatisticsController(IUserStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Get comprehensive statistics for a user.
    /// </summary>
    [HttpGet("{userId}")]
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
    /// Get subtopic performance breakdown for a user.
    /// </summary>
    [HttpGet("{userId}/subtopics")]
    public async Task<ActionResult<IEnumerable<SubtopicPerformanceDto>>> GetSubtopicPerformance(
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
    /// Get session history for a user.
    /// </summary>
    [HttpGet("{userId}/sessions")]
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

    /// <summary>
    /// Trigger statistics update after session completion.
    /// </summary>
    [HttpPost("{userId}/sessions/{sessionId}/update-stats")]
    public async Task<ActionResult> UpdateStatisticsAfterSession(string userId, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest("Session ID is required.");
        }

        try
        {
            await _statisticsService.UpdateStatisticsAfterSessionAsync(userId, sessionId);
            return Ok("Statistics updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating statistics: {ex.Message}");
        }
    }
}