using Microsoft.AspNetCore.Identity;
using Po.LearnCert.Api.Features.Authentication.Infrastructure;
using Po.LearnCert.Shared.Contracts;

namespace Po.LearnCert.Api.Features.Authentication.Services;

/// <summary>
/// Service for handling user authentication operations (register, login, logout).
/// </summary>
public class AuthenticationService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account with password validation.
    /// </summary>
    /// <param name="request">Registration request containing email, username, and password.</param>
    /// <returns>Authentication response indicating success or failure with error messages.</returns>
    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting to register user: {Username}", request.Username);

        // Validate password confirmation
        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Registration failed for {Username}: Password mismatch", request.Username);
            return AuthenticationResponse.Failure("Passwords do not match");
        }

        // Create new user
        var user = new UserEntity
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Username} registered successfully", request.Username);
            return AuthenticationResponse.Success(user.Id, user.UserName!, user.Email!);
        }

        // Return identity errors
        var errors = result.Errors.Select(e => e.Description).ToArray();
        _logger.LogWarning("Registration failed for {Username}: {Errors}", request.Username, string.Join(", ", errors));
        return AuthenticationResponse.Failure(errors);
    }

    /// <summary>
    /// Authenticates a user and creates a session.
    /// </summary>
    /// <param name="request">Login request containing username and password.</param>
    /// <returns>Authentication response indicating success or failure.</returns>
    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Attempting to login user: {Username}", request.Username);

        // Find user by username
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User {Username} not found", request.Username);
            return AuthenticationResponse.Failure("Invalid username or password");
        }

        // Attempt sign in
        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return AuthenticationResponse.Success(user.Id, user.UserName!, user.Email!);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login failed: User {Username} is locked out", request.Username);
            return AuthenticationResponse.Failure("Account is locked out");
        }

        if (result.RequiresTwoFactor)
        {
            _logger.LogWarning("Login failed: User {Username} requires two-factor authentication", request.Username);
            return AuthenticationResponse.Failure("Two-factor authentication required");
        }

        _logger.LogWarning("Login failed for {Username}: Invalid credentials", request.Username);
        return AuthenticationResponse.Failure("Invalid username or password");
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    public async Task LogoutAsync()
    {
        _logger.LogInformation("User logout requested");
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out successfully");
    }
}
