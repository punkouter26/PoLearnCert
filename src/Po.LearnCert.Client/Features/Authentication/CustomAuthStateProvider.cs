using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Po.LearnCert.Client.Features.Authentication;

/// <summary>
/// Custom authentication state provider for managing user authentication state in Blazor WebAssembly.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private ClaimsPrincipal? _currentUser;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser ?? _anonymous));
    }

    /// <summary>
    /// Marks the user as authenticated and updates the authentication state.
    /// </summary>
    /// <param name="username">The authenticated user's username.</param>
    public Task MarkUserAsAuthenticated(string username)
    {
        var authenticatedUser = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, "apiauth"));

        _currentUser = authenticatedUser;

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks the user as logged out and updates the authentication state.
    /// </summary>
    public void MarkUserAsLoggedOut()
    {
        _currentUser = null;

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
