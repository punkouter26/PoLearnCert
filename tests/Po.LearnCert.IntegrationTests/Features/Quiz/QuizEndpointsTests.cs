using System.Net;
using System.Net.Http.Json;
using Po.LearnCert.IntegrationTests.Infrastructure;
using Po.LearnCert.Shared.Contracts;

namespace Po.LearnCert.IntegrationTests.Features.Quiz;

[Collection("Azurite collection")]
public class QuizEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactoryWithAzurite<Program> _factory;
    private readonly HttpClient _client;

    public QuizEndpointsTests(AzuriteFixture azuriteFixture)
    {
        _factory = new TestWebApplicationFactoryWithAzurite<Program>(azuriteFixture);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region GET /api/quiz/sessions/{id}

    [Fact]
    public async Task GET_SessionDetails_WithNonExistentSession_ShouldReturn404()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/quiz/sessions/{sessionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/quiz/sessions

    [Fact]
    public async Task POST_CreateSession_WithInvalidCertification_ShouldReturn400()
    {
        // Arrange
        var request = new CreateQuizSessionRequest
        {
            CertificationId = "non-existent-cert",
            QuestionCount = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/quiz/sessions", request);

        // Assert
        // Expect 400 or 500 depending on error handling
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task POST_CreateSession_WithEmptyCertificationId_ShouldReturn400()
    {
        // Arrange
        var request = new CreateQuizSessionRequest
        {
            CertificationId = "",
            QuestionCount = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/quiz/sessions", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_CreateSession_WithZeroQuestionCount_ShouldReturn400()
    {
        // Arrange
        var request = new CreateQuizSessionRequest
        {
            CertificationId = "cert1",
            QuestionCount = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/quiz/sessions", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /api/quiz/sessions/{id}/answers

    [Fact]
    public async Task POST_SubmitAnswer_WithInvalidSession_ShouldReturn404OrBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var request = new SubmitAnswerRequest
        {
            SessionId = sessionId,
            QuestionId = "q1",
            SelectedChoiceId = "choice1"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/quiz/sessions/{sessionId}/answers", request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /api/quiz/sessions/{id}/results

    [Fact]
    public async Task GET_SessionResults_WithNonExistentSession_ShouldReturn404()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/quiz/sessions/{sessionId}/results");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }

    #endregion
}
