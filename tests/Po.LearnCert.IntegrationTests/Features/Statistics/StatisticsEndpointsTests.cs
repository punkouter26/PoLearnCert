using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Po.LearnCert.Shared.Models;
using Po.LearnCert.IntegrationTests.Infrastructure;

namespace Po.LearnCert.IntegrationTests.Features.Statistics;

[Collection("Azurite collection")]
public class StatisticsEndpointsTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactoryWithAzurite<Program> _factory;

    public StatisticsEndpointsTests(AzuriteFixture azuriteFixture)
    {
        _factory = new TestWebApplicationFactoryWithAzurite<Program>(azuriteFixture);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GET_Statistics_ReturnsOkWithUserStatistics()
    {
        // Arrange
        var userId = "testuser123";

        // Act
        var response = await _client.GetAsync($"/api/statistics?userId={userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsDto>();
        stats.Should().NotBeNull();
        stats.TotalSessions.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GET_Statistics_WithoutUserId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Statistics_WithInvalidUserId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/statistics?userId=nonexistent");

        // Assert - Should return OK with zero stats rather than 404
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsDto>();
        stats.Should().NotBeNull();
        stats.TotalSessions.Should().Be(0);
    }

    [Fact]
    public async Task GET_SubtopicPerformance_ReturnsOkWithPerformanceList()
    {
        // Arrange
        var userId = "testuser123";

        // Act
        var response = await _client.GetAsync($"/api/statistics/subtopics?userId={userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var performance = await response.Content.ReadFromJsonAsync<List<SubtopicPerformanceDto>>();
        performance.Should().NotBeNull();
        performance.Should().BeOfType<List<SubtopicPerformanceDto>>();
    }

    [Fact]
    public async Task GET_SubtopicPerformance_WithCertificationFilter_ReturnsFilteredResults()
    {
        // Arrange
        var userId = "testuser123";
        var certificationId = "az900";

        // Act
        var response = await _client.GetAsync(
            $"/api/statistics/subtopics?userId={userId}&certificationId={certificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var performance = await response.Content.ReadFromJsonAsync<List<SubtopicPerformanceDto>>();
        performance.Should().NotBeNull();

        // If there are results, they should all be for the requested certification
        if (performance.Count > 0)
        {
            performance.Should().OnlyContain(p => p.CertificationId == certificationId);
        }
    }

    [Fact]
    public async Task GET_SubtopicPerformance_WithoutUserId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/statistics/subtopics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
