using Microsoft.AspNetCore.Mvc;

namespace Po.LearnCert.Api.Features.Health;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for readiness and liveness probes.
    /// </summary>
    /// <returns>200 OK if the API is healthy</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        var health = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        _logger.LogDebug("Health check performed: {Status}", health.Status);

        return Ok(health);
    }
}
