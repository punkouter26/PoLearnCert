using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Services;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Statistics;

/// <summary>
/// Controller for statistics endpoints (query parameter based).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IUserStatisticsService _statisticsService;

    public StatisticsController(IUserStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Get comprehensive statistics for a user via query parameter.
    /// </summary>
    [HttpGet]
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
    /// Get subtopic performance breakdown for a user via query parameters.
    /// </summary>
    [HttpGet("subtopics")]
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
}
