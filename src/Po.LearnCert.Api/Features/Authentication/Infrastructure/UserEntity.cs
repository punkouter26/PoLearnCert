using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Authentication.Infrastructure;

/// <summary>
/// User entity for Azure Table Storage, compatible with ASP.NET Core Identity.
/// </summary>
public class UserEntity : TableEntityBase
{
    public UserEntity()
    {
        // PartitionKey = "USER" for all users
        // RowKey = UserId (normalized username or email)
    }

    /// <summary>
    /// Unique user identifier (same as RowKey).
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Username for login.
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// Normalized username for case-insensitive lookups.
    /// </summary>
    public string NormalizedUserName { get; set; } = default!;

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Normalized email for case-insensitive lookups.
    /// </summary>
    public string? NormalizedEmail { get; set; }

    /// <summary>
    /// Indicates if the email is confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Hashed password.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Security stamp for invalidating tokens when credentials change.
    /// </summary>
    public string? SecurityStamp { get; set; }

    /// <summary>
    /// Concurrency stamp for optimistic concurrency.
    /// </summary>
    public string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates if the phone number is confirmed.
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// Indicates if two-factor authentication is enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// UTC date/time when lockout ends.
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// Indicates if lockout is enabled for this user.
    /// </summary>
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// Number of failed login attempts.
    /// </summary>
    public int AccessFailedCount { get; set; }
}
