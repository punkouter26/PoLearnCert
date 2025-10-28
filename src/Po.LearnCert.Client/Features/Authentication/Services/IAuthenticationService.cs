using Po.LearnCert.Shared.Contracts;

namespace Po.LearnCert.Client.Features.Authentication.Services;

/// <summary>
/// Interface for authentication operations in the client application.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>Authentication response with success status and user details or errors.</returns>
    Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user and creates a session.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Authentication response with session details or error.</returns>
    Task<AuthenticationResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Signs out the current authenticated user.
    /// </summary>
    /// <returns>Task representing the async operation.</returns>
    Task LogoutAsync();

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User details if authenticated, null otherwise.</returns>
    Task<UserInfo?> GetCurrentUserAsync();
}

/// <summary>
/// Simple user information model for frontend use.
/// </summary>
public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}
