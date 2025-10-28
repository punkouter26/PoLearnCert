using System.ComponentModel.DataAnnotations;

namespace Po.LearnCert.Shared.Contracts;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username for authentication.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to remember the user's session (persist authentication cookie).
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
