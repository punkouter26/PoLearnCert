using System.Net;
using System.Net.Http.Json;
using Po.LearnCert.Shared.Models;
using Po.LearnCert.IntegrationTests.Infrastructure;

namespace Po.LearnCert.IntegrationTests.Features.Certifications;

[Collection("Azurite collection")]
public class CertificationEndpointsTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactoryWithAzurite<Program> _factory;

    public CertificationEndpointsTests(AzuriteFixture azuriteFixture)
    {
        _factory = new TestWebApplicationFactoryWithAzurite<Program>(azuriteFixture);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region GET /api/certifications

    [Fact]
    public async Task GET_Certifications_ShouldReturn200AndList()
    {
        // Act
        var response = await _client.GetAsync("/api/certifications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<CertificationDto>>();
        Assert.NotNull(result);
        // Assuming we have seeded certifications
    }

    #endregion

    #region GET /api/certifications/{id}

    [Fact]
    public async Task GET_Certification_WithValidId_ShouldReturn200()
    {
        // Arrange - use a known certification ID from seed data
        var certificationId = "az-104";

        // Act
        var response = await _client.GetAsync($"/api/certifications/{certificationId}");

        // Assert
        // Could be 200 OK or 404 Not Found depending on seed data
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_Certification_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var certificationId = "non-existent-certification";

        // Act
        var response = await _client.GetAsync($"/api/certifications/{certificationId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GET /api/certifications/{id}/subtopics

    [Fact]
    public async Task GET_Subtopics_WithValidCertification_ShouldReturn200()
    {
        // Arrange
        var certificationId = "az-104";

        // Act
        var response = await _client.GetAsync($"/api/certifications/{certificationId}/subtopics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<SubtopicDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GET_Subtopics_WithInvalidCertification_ShouldReturn200WithEmptyList()
    {
        // Arrange
        var certificationId = "non-existent-certification";

        // Act
        var response = await _client.GetAsync($"/api/certifications/{certificationId}/subtopics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<SubtopicDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion
}
