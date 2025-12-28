using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Po.LearnCert.Api.Features.QuestionGeneration;

namespace Po.LearnCert.UnitTests.Features.QuestionGeneration;

public class QuestionGenerationControllerTests
{
    private readonly Mock<IQuestionGenerationService> _generationServiceMock;
    private readonly Mock<ILogger<QuestionGenerationController>> _loggerMock;
    private readonly QuestionGenerationController _controller;

    public QuestionGenerationControllerTests()
    {
        _generationServiceMock = new Mock<IQuestionGenerationService>();
        _loggerMock = new Mock<ILogger<QuestionGenerationController>>();
        _controller = new QuestionGenerationController(
            _generationServiceMock.Object,
            _loggerMock.Object);
    }

    #region GenerateForSubtopic Tests

    [Fact]
    public async Task GenerateForSubtopic_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new GenerateForSubtopicRequest(
            CertificationId: "AZ900",
            SubtopicId: "subtopic1",
            SubtopicName: "Cloud Concepts",
            Count: 5);

        var generationResult = new QuestionGenerationResult(
            TotalGenerated: 5,
            TotalFailed: 0,
            Duration: TimeSpan.FromSeconds(10),
            Errors: new List<string>());

        _generationServiceMock
            .Setup(x => x.GenerateQuestionsAsync(
                request.CertificationId,
                request.SubtopicId,
                request.SubtopicName,
                request.Count,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generationResult);

        // Act
        var result = await _controller.GenerateForSubtopic(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<GenerationResultDto>(okResult.Value);
        Assert.Equal(5, dto.TotalGenerated);
        Assert.Equal(0, dto.TotalFailed);
        Assert.Equal(10, dto.DurationSeconds);
        Assert.Empty(dto.Errors);
    }

    [Fact]
    public async Task GenerateForSubtopic_WithPartialFailures_ReturnsResultWithErrors()
    {
        // Arrange
        var request = new GenerateForSubtopicRequest(
            CertificationId: "AZ900",
            SubtopicId: "subtopic1",
            SubtopicName: "Cloud Concepts",
            Count: 10);

        var errors = new List<string>
        {
            "Question 3: API rate limit exceeded",
            "Question 7: Invalid JSON response"
        };

        var generationResult = new QuestionGenerationResult(
            TotalGenerated: 8,
            TotalFailed: 2,
            Duration: TimeSpan.FromSeconds(30),
            Errors: errors);

        _generationServiceMock
            .Setup(x => x.GenerateQuestionsAsync(
                request.CertificationId,
                request.SubtopicId,
                request.SubtopicName,
                request.Count,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generationResult);

        // Act
        var result = await _controller.GenerateForSubtopic(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<GenerationResultDto>(okResult.Value);
        Assert.Equal(8, dto.TotalGenerated);
        Assert.Equal(2, dto.TotalFailed);
        Assert.Equal(2, dto.Errors.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GenerateForSubtopic_WithInvalidCount_ReturnsBadRequest(int count)
    {
        // Arrange
        var request = new GenerateForSubtopicRequest(
            CertificationId: "AZ900",
            SubtopicId: "subtopic1",
            SubtopicName: "Cloud Concepts",
            Count: count);

        // Act
        var result = await _controller.GenerateForSubtopic(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GenerateForSubtopic_TruncatesErrorsToTen()
    {
        // Arrange
        var request = new GenerateForSubtopicRequest(
            CertificationId: "AZ900",
            SubtopicId: "subtopic1",
            SubtopicName: "Cloud Concepts",
            Count: 20);

        var errors = Enumerable.Range(1, 15)
            .Select(i => $"Error {i}")
            .ToList();

        var generationResult = new QuestionGenerationResult(
            TotalGenerated: 5,
            TotalFailed: 15,
            Duration: TimeSpan.FromSeconds(60),
            Errors: errors);

        _generationServiceMock
            .Setup(x => x.GenerateQuestionsAsync(
                request.CertificationId,
                request.SubtopicId,
                request.SubtopicName,
                request.Count,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generationResult);

        // Act
        var result = await _controller.GenerateForSubtopic(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<GenerationResultDto>(okResult.Value);
        Assert.Equal(10, dto.Errors.Count);
    }

    #endregion

    #region GenerateForCertification Tests

    [Fact]
    public async Task GenerateForCertification_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new GenerateForCertificationRequest(
            CertificationId: "AZ900",
            QuestionsPerSubtopic: 5);

        var generationResult = new QuestionGenerationResult(
            TotalGenerated: 25,
            TotalFailed: 0,
            Duration: TimeSpan.FromMinutes(2),
            Errors: new List<string>());

        _generationServiceMock
            .Setup(x => x.GenerateQuestionsForCertificationAsync(
                request.CertificationId,
                request.QuestionsPerSubtopic,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generationResult);

        // Act
        var result = await _controller.GenerateForCertification(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<GenerationResultDto>(okResult.Value);
        Assert.Equal(25, dto.TotalGenerated);
        Assert.Equal(0, dto.TotalFailed);
        Assert.Equal(120, dto.DurationSeconds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public async Task GenerateForCertification_WithInvalidQuestionsPerSubtopic_ReturnsBadRequest(int questionsPerSubtopic)
    {
        // Arrange
        var request = new GenerateForCertificationRequest(
            CertificationId: "AZ900",
            QuestionsPerSubtopic: questionsPerSubtopic);

        // Act
        var result = await _controller.GenerateForCertification(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GenerateForCertification_WithBoundaryCount_ReturnsOkResult()
    {
        // Arrange - test boundary value of 100
        var request = new GenerateForCertificationRequest(
            CertificationId: "AZ900",
            QuestionsPerSubtopic: 100);

        var generationResult = new QuestionGenerationResult(
            TotalGenerated: 500,
            TotalFailed: 0,
            Duration: TimeSpan.FromMinutes(10),
            Errors: new List<string>());

        _generationServiceMock
            .Setup(x => x.GenerateQuestionsForCertificationAsync(
                request.CertificationId,
                request.QuestionsPerSubtopic,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generationResult);

        // Act
        var result = await _controller.GenerateForCertification(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    #endregion
}
