using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Features.Leaderboards.Services;

namespace Po.LearnCert.Api.Features.Leaderboards;

/// <summary>
/// Controller for leaderboard endpoints.
/// </summary>
[ApiController]
[Route("api/leaderboards")]
public class LeaderboardsController : ControllerBase
{
    private readonly LeaderboardService _leaderboardService;
    private readonly ILogger<LeaderboardsController> _logger;

    public LeaderboardsController(
        LeaderboardService leaderboardService,
        ILogger<LeaderboardsController> logger)
    {
        _leaderboardService = leaderboardService;
        _logger = logger;
    }

    /// <summary>
    /// Gets leaderboard for a specific certification.
    /// Supports filtering by time period and pagination (FR-011, FR-012, FR-013).
    /// </summary>
    /// <remarks>
    /// This query-string variant exists for compatibility with existing clients/tests.
    /// It returns only the leaderboard entries.
    /// </remarks>
    /// <param name="certificationId">Certification identifier</param>
    /// <param name="timePeriod">Time period filter (AllTime, Monthly, Weekly). Default: AllTime</param>
    /// <param name="skip">Number of entries to skip for pagination. Default: 0</param>
    /// <param name="limit">Number of entries to take. Default: 50, Max: 100</param>
    /// <param name="userId">Optional current user ID to highlight their entry</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeaderboardQuery(
        [FromQuery] string? certificationId,
        [FromQuery] string timePeriod = "AllTime",
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50,
        [FromQuery] string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(certificationId))
        {
            return BadRequest(new { error = "Certification ID is required." });
        }

        if (skip < 0)
        {
            return BadRequest(new { error = "Skip parameter must be non-negative." });
        }

        if (limit < 1 || limit > 100)
        {
            return BadRequest(new { error = "Limit parameter must be between 1 and 100." });
        }

        var validTimePeriods = new[] { "AllTime", "Monthly", "Weekly" };
        if (!validTimePeriods.Contains(timePeriod, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                error = $"Invalid time period '{timePeriod}'. Must be one of: {string.Join(", ", validTimePeriods)}"
            });
        }

        try
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(
                certificationId,
                timePeriod,
                skip,
                limit,
                userId);

            return Ok(leaderboard.Entries);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for leaderboard request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard for certification {CertId}", certificationId);
            return StatusCode(500, new
            {
                error = "An error occurred while retrieving the leaderboard.",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Gets leaderboard for a specific certification.
    /// Supports filtering by time period and pagination (FR-011, FR-012, FR-013).
    /// </summary>
    /// <param name="certId">Certification identifier</param>
    /// <param name="timePeriod">Time period filter (AllTime, Monthly, Weekly). Default: AllTime</param>
    /// <param name="skip">Number of entries to skip for pagination. Default: 0</param>
    /// <param name="take">Number of entries to take for pagination. Default: 50, Max: 100</param>
    /// <param name="userId">Optional current user ID to highlight their entry</param>
    [HttpGet("{certId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeaderboard(
        string certId,
        [FromQuery] string timePeriod = "AllTime",
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? userId = null)
    {
        try
        {
            _logger.LogInformation(
                "GET /api/leaderboards/{CertId}?timePeriod={TimePeriod}&skip={Skip}&take={Take}",
                certId, timePeriod, skip, take);

            // Validate parameters
            if (string.IsNullOrWhiteSpace(certId))
            {
                return BadRequest(new { error = "Certification ID is required." });
            }

            if (skip < 0)
            {
                return BadRequest(new { error = "Skip parameter must be non-negative." });
            }

            if (take < 1 || take > 100)
            {
                return BadRequest(new { error = "Take parameter must be between 1 and 100." });
            }

            var validTimePeriods = new[] { "AllTime", "Monthly", "Weekly" };
            if (!validTimePeriods.Contains(timePeriod, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    error = $"Invalid time period '{timePeriod}'. Must be one of: {string.Join(", ", validTimePeriods)}"
                });
            }

            // Get leaderboard
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(
                certId,
                timePeriod,
                skip,
                take,
                userId);

            return Ok(leaderboard);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for leaderboard request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard for certification {CertId}", certId);
            return StatusCode(500, new
            {
                error = "An error occurred while retrieving the leaderboard.",
                details = ex.Message
            });
        }
    }
}
