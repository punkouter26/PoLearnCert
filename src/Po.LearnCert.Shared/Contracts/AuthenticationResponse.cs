namespace Po.LearnCert.Shared.Contracts;

/// <summary>
/// Response model for authentication operations (register, login).
/// </summary>
public class AuthenticationResponse
{
    /// <summary>
    /// Indicates whether the authentication operation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Username of the authenticated user (if successful).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Email of the authenticated user (if successful).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User ID (if successful).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// List of error messages (if operation failed).
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful authentication response.
    /// </summary>
    public static AuthenticationResponse Success(string userId, string username, string email)
    {
        return new AuthenticationResponse
        {
            Succeeded = true,
            UserId = userId,
            Username = username,
            Email = email
        };
    }

    /// <summary>
    /// Creates a failed authentication response with error messages.
    /// </summary>
    public static AuthenticationResponse Failure(params string[] errors)
    {
        return new AuthenticationResponse
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }
}
