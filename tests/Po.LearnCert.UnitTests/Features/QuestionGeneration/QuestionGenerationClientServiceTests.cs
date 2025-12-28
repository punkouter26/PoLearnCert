using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Po.LearnCert.Client.Features.QuestionGeneration;

namespace Po.LearnCert.UnitTests.Features.QuestionGeneration;

public class QuestionGenerationClientServiceTests
{
    private readonly Mock<ILogger<QuestionGenerationService>> _loggerMock;

    public QuestionGenerationClientServiceTests()
    {
        _loggerMock = new Mock<ILogger<QuestionGenerationService>>();
    }

    private (HttpClient, Mock<HttpMessageHandler>) CreateMockHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://test.example.com/")
        };
        return (httpClient, handlerMock);
    }

    #region GenerateForSubtopicAsync Tests

    [Fact]
    public async Task GenerateForSubtopicAsync_WithValidRequest_ReturnsResult()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();

        var expectedResult = new GenerationResultDto(
            TotalGenerated: 5,
            TotalFailed: 0,
            DurationSeconds: 10.5,
            Errors: new List<string>());

        var responseContent = JsonSerializer.Serialize(expectedResult);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.PathAndQuery == "/api/questions/generate/subtopic"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GenerateForSubtopicAsync(
            "AZ900",
            "subtopic1",
            "Cloud Concepts",
            5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalGenerated);
        Assert.Equal(0, result.TotalFailed);
        Assert.Equal(10.5, result.DurationSeconds);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task GenerateForSubtopicAsync_WithPartialFailures_ReturnsResultWithErrors()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();

        var expectedResult = new GenerationResultDto(
            TotalGenerated: 3,
            TotalFailed: 2,
            DurationSeconds: 15.0,
            Errors: new List<string> { "Error 1", "Error 2" });

        var responseContent = JsonSerializer.Serialize(expectedResult);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GenerateForSubtopicAsync(
            "AZ900",
            "subtopic1",
            "Cloud Concepts",
            5);

        // Assert
        Assert.Equal(3, result.TotalGenerated);
        Assert.Equal(2, result.TotalFailed);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public async Task GenerateForSubtopicAsync_WithHttpError_ThrowsException()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server error")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GenerateForSubtopicAsync("AZ900", "subtopic1", "Cloud Concepts", 5));
    }

    [Fact]
    public async Task GenerateForSubtopicAsync_SendsCorrectRequest()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();
        HttpRequestMessage? capturedRequest = null;

        var expectedResult = new GenerationResultDto(5, 0, 10.0, new List<string>());
        var responseContent = JsonSerializer.Serialize(expectedResult);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act
        await service.GenerateForSubtopicAsync("AZ900", "subtopic1", "Cloud Concepts", 5);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Equal("/api/questions/generate/subtopic", capturedRequest.RequestUri?.PathAndQuery);

        var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("AZ900", requestBody);
        Assert.Contains("subtopic1", requestBody);
        Assert.Contains("Cloud Concepts", requestBody);
    }

    [Fact]
    public async Task GenerateForSubtopicAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act & Assert
        // TaskCanceledException inherits from OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GenerateForSubtopicAsync("AZ900", "subtopic1", "Cloud Concepts", 5, cts.Token));
    }

    #endregion

    #region GenerateForCertificationAsync Tests

    [Fact]
    public async Task GenerateForCertificationAsync_WithValidRequest_ReturnsResult()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();

        var expectedResult = new GenerationResultDto(
            TotalGenerated: 25,
            TotalFailed: 0,
            DurationSeconds: 120.0,
            Errors: new List<string>());

        var responseContent = JsonSerializer.Serialize(expectedResult);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.PathAndQuery == "/api/questions/generate/certification"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.GenerateForCertificationAsync("AZ900", 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.TotalGenerated);
        Assert.Equal(0, result.TotalFailed);
        Assert.Equal(120.0, result.DurationSeconds);
    }

    [Fact]
    public async Task GenerateForCertificationAsync_SendsCorrectRequest()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();
        HttpRequestMessage? capturedRequest = null;

        var expectedResult = new GenerationResultDto(25, 0, 120.0, new List<string>());
        var responseContent = JsonSerializer.Serialize(expectedResult);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act
        await service.GenerateForCertificationAsync("SECPLUS", 10);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Equal("/api/questions/generate/certification", capturedRequest.RequestUri?.PathAndQuery);

        var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("SECPLUS", requestBody);
        Assert.Contains("10", requestBody);
    }

    [Fact]
    public async Task GenerateForCertificationAsync_WithHttpError_ThrowsException()
    {
        // Arrange
        var (httpClient, handlerMock) = CreateMockHttpClient();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad request")
            });

        var service = new QuestionGenerationService(httpClient, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GenerateForCertificationAsync("AZ900", 5));
    }

    #endregion
}
