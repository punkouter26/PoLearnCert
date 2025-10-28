using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Features.Authentication.Services;
using Po.LearnCert.Shared.Contracts;

namespace Po.LearnCert.Api.Features.Authentication;

/// <summary>
/// Controller for authentication operations (register, login, logout).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthenticationService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details (email, username, password).</param>
    /// <returns>Authentication response with success status and user details or errors.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Invalid request or registration failed.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration request validation failed");
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(request);

            if (result.Succeeded)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return Problem(
                title: "Registration Error",
                detail: "An error occurred while processing your registration",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Authenticates a user and creates a session.
    /// </summary>
    /// <param name="request">Login credentials (username, password).</param>
    /// <returns>Authentication response with session details or error.</returns>
    /// <response code="200">User authenticated successfully.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login request validation failed");
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);

            if (result.Succeeded)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return Problem(
                title: "Login Error",
                detail: "An error occurred while processing your login",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Signs out the current authenticated user.
    /// </summary>
    /// <returns>Success status.</returns>
    /// <response code="200">User logged out successfully.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _authService.LogoutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user logout");
            return Problem(
                title: "Logout Error",
                detail: "An error occurred while processing your logout",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User details if authenticated.</returns>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var username = User.Identity?.Name;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return Ok(new
            {
                userId,
                username,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return Problem(
                title: "User Retrieval Error",
                detail: "An error occurred while retrieving user information",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
