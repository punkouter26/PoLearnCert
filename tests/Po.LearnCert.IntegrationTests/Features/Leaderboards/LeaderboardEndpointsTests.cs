using System.Net;
using System.Net.Http.Json;
using Po.LearnCert.Shared.Models;
using Po.LearnCert.IntegrationTests.Infrastructure;

namespace Po.LearnCert.IntegrationTests.Features.Leaderboards;

[Collection("Azurite collection")]
public class LeaderboardEndpointsTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactoryWithAzurite<Program> _factory;

    public LeaderboardEndpointsTests(AzuriteFixture azuriteFixture)
    {
        _factory = new TestWebApplicationFactoryWithAzurite<Program>(azuriteFixture);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region GET /api/leaderboards

    [Fact]
    public async Task GET_Leaderboard_WithValidParameters_ShouldReturn200()
    {
        // Arrange
        var certificationId = "az-104"; // Use a known certification
        var timePeriod = "AllTime";

        // Act
        var response = await _client.GetAsync($"/api/leaderboards?certificationId={certificationId}&timePeriod={timePeriod}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_Leaderboard_WithoutCertificationId_ShouldReturn400()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboards");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Leaderboard_WithInvalidTimePeriod_ShouldReturn400()
    {
        // Arrange
        var certificationId = "az-104";

        // Act
        var response = await _client.GetAsync($"/api/leaderboards?certificationId={certificationId}&timePeriod=InvalidPeriod");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Leaderboard_WithAllTimePeriods_ShouldReturn200()
    {
        // Arrange
        var certificationId = "az-104";
        var periods = new[] { "AllTime", "Monthly", "Weekly" };

        foreach (var period in periods)
        {
            // Act
            var response = await _client.GetAsync($"/api/leaderboards?certificationId={certificationId}&timePeriod={period}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task GET_Leaderboard_WithLimitParameter_ShouldReturn200()
    {
        // Arrange
        var certificationId = "az-104";
        var limit = 5;

        // Act
        var response = await _client.GetAsync($"/api/leaderboards?certificationId={certificationId}&timePeriod=AllTime&limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<LeaderboardEntryDto>>();
        Assert.NotNull(result);
        Assert.True(result.Count() <= limit);
    }

    #endregion
}
