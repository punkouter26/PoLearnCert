using Po.LearnCert.Shared.Models;
using System.Net.Http.Json;

namespace Po.LearnCert.Client.Features.Certifications.Services;

/// <summary>
/// HTTP client service for certification operations.
/// </summary>
public class CertificationService : ICertificationService
{
    private readonly HttpClient _httpClient;

    public CertificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<List<CertificationDto>> GetAllCertificationsAsync()
    {
        var response = await _httpClient.GetAsync("api/certifications");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CertificationDto>>()
            ?? new List<CertificationDto>();
    }

    /// <inheritdoc/>
    public async Task<CertificationDto?> GetCertificationByIdAsync(string certificationId)
    {
        var response = await _httpClient.GetAsync($"api/certifications/{certificationId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<CertificationDto>();
    }

    /// <inheritdoc/>
    public async Task<List<SubtopicDto>> GetSubtopicsAsync(string certificationId)
    {
        var response = await _httpClient.GetAsync($"api/certifications/{certificationId}/subtopics");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SubtopicDto>>()
            ?? new List<SubtopicDto>();
    }
}
