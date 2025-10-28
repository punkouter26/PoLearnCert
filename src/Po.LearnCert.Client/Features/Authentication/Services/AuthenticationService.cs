using System.Net.Http.Json;
using Po.LearnCert.Shared.Contracts;

namespace Po.LearnCert.Client.Features.Authentication.Services;

/// <summary>
/// HTTP client wrapper for authentication operations.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthenticationService(
        HttpClient httpClient,
        CustomAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (result?.Succeeded == true)
            {
                // Notify auth state provider of successful registration
                await _authStateProvider.MarkUserAsAuthenticated(result.Username!);
            }
            return result ?? AuthenticationResponse.Failure("Empty response from server");
        }

        var errorResult = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        return errorResult ?? AuthenticationResponse.Failure("Registration failed");
    }

    /// <summary>
    /// Authenticates a user and creates a session.
    /// </summary>
    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (result?.Succeeded == true)
            {
                // Notify auth state provider of successful login
                await _authStateProvider.MarkUserAsAuthenticated(result.Username!);
            }
            return result ?? AuthenticationResponse.Failure("Empty response from server");
        }

        var errorResult = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        return errorResult ?? AuthenticationResponse.Failure("Login failed");
    }

    /// <summary>
    /// Signs out the current authenticated user.
    /// </summary>
    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("/api/auth/logout", null);
        
        // Notify auth state provider of logout
        _authStateProvider.MarkUserAsLoggedOut();
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfo>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
