using Xunit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Po.LearnCert.Api.Features.Statistics;
using Po.LearnCert.Api.Features.Statistics.Services;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.UnitTests.Controllers;

public class UserStatisticsControllerTests
{
    private readonly Mock<IUserStatisticsService> _mockService;
    private readonly StatisticsController _controller;

    public UserStatisticsControllerTests()
    {
        _mockService = new Mock<IUserStatisticsService>();
        _controller = new StatisticsController(_mockService.Object);
    }

    [Fact]
    public async Task GetUserStatistics_WithValidUserId_ReturnsOkResult()
    {
        // Arrange
        var userId = "test-user";
        var statistics = new UserStatisticsDto
        {
            UserId = userId,
            TotalSessions = 5,
            TotalQuestionsAnswered = 50,
            TotalCorrectAnswers = 40,
            OverallAccuracy = 80.0m
        };

        _mockService.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetUserStatistics(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStats = Assert.IsType<UserStatisticsDto>(okResult.Value);
        Assert.Equal(userId, returnedStats.UserId);
        Assert.Equal(5, returnedStats.TotalSessions);
        Assert.Equal(80.0m, returnedStats.OverallAccuracy);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetUserStatistics_WithInvalidUserId_ReturnsBadRequest(string userId)
    {
        // Act
        var result = await _controller.GetUserStatistics(userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetStatistics_WithNullUserId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetStatistics(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserStatistics_WhenServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        var userId = "test-user";
        _mockService.Setup(x => x.GetUserStatisticsAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserStatistics(userId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Database error", statusResult.Value?.ToString());
    }

    [Fact]
    public async Task GetSubtopicPerformance_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var userId = "test-user";
        var certificationId = "AZ-900";
        var performance = new List<SubtopicPerformanceDto>
        {
            new()
            {
                SubtopicId = "cloud-concepts",
                SubtopicName = "Cloud Concepts",
                CertificationId = certificationId,
                QuestionsAnswered = 10,
                CorrectAnswers = 8,
                Accuracy = 80.0m,
                PerformanceLevel = PerformanceLevel.Good
            }
        };

        _mockService.Setup(x => x.GetSubtopicPerformanceAsync(userId, certificationId))
            .ReturnsAsync(performance);

        // Act
        var result = await _controller.GetUserSubtopicPerformance(userId, certificationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPerformance = Assert.IsType<List<SubtopicPerformanceDto>>(okResult.Value);
        Assert.Single(returnedPerformance);
        Assert.Equal("cloud-concepts", returnedPerformance.First().SubtopicId);
    }

    [Fact]
    public async Task GetSubtopicPerformance_WithoutCertificationId_ReturnsAllSubtopics()
    {
        // Arrange
        var userId = "test-user";
        var performance = new List<SubtopicPerformanceDto>
        {
            new()
            {
                SubtopicId = "cloud-concepts",
                CertificationId = "AZ-900",
                SubtopicName = "Cloud Concepts"
            },
            new()
            {
                SubtopicId = "security-fundamentals",
                CertificationId = "Security+",
                SubtopicName = "Security Fundamentals"
            }
        };

        _mockService.Setup(x => x.GetSubtopicPerformanceAsync(userId, null))
            .ReturnsAsync(performance);

        // Act
        var result = await _controller.GetUserSubtopicPerformance(userId, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPerformance = Assert.IsType<List<SubtopicPerformanceDto>>(okResult.Value);
        Assert.Equal(2, returnedPerformance.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetSubtopicPerformance_WithInvalidUserId_ReturnsBadRequest(string userId)
    {
        // Act
        var result = await _controller.GetUserSubtopicPerformance(userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetSessionHistory_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var userId = "test-user";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var limit = 10;

        var sessionHistory = new SessionHistoryDto
        {
            UserId = userId,
            Sessions = new List<SessionSummaryDto>
            {
                new()
                {
                    SessionId = "session-1",
                    CertificationId = "AZ-900",
                    CertificationName = "Azure Fundamentals",
                    StartedAt = DateTime.UtcNow.AddDays(-1),
                    CompletedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(15),
                    TotalQuestions = 5,
                    QuestionsAnswered = 5,
                    CorrectAnswers = 4,
                    ScorePercentage = 80.0m,
                    Status = SessionStatus.Completed
                }
            },
            TotalSessions = 1,
            DateRange = new DateRange { StartDate = startDate, EndDate = endDate }
        };

        _mockService.Setup(x => x.GetSessionHistoryAsync(userId, It.IsAny<DateRange>(), limit))
            .ReturnsAsync(sessionHistory);

        // Act
        var result = await _controller.GetSessionHistory(userId, startDate, endDate, limit);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedHistory = Assert.IsType<SessionHistoryDto>(okResult.Value);
        Assert.Equal(userId, returnedHistory.UserId);
        Assert.Single(returnedHistory.Sessions);
        Assert.Equal(1, returnedHistory.TotalSessions);
    }

    [Fact]
    public async Task GetSessionHistory_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var userId = "test-user";
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1); // End before start

        // Act
        var result = await _controller.GetSessionHistory(userId, startDate, endDate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Start date cannot be after end date.", badRequestResult.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetSessionHistory_WithInvalidLimit_ReturnsBadRequest(int limit)
    {
        // Arrange
        var userId = "test-user";

        // Act
        var result = await _controller.GetSessionHistory(userId, null, null, limit);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Limit must be greater than 0.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetSessionHistory_WithoutDateRange_ReturnsAllSessions()
    {
        // Arrange
        var userId = "test-user";
        var sessionHistory = new SessionHistoryDto
        {
            UserId = userId,
            Sessions = new List<SessionSummaryDto>(),
            TotalSessions = 0,
            DateRange = null
        };

        _mockService.Setup(x => x.GetSessionHistoryAsync(userId, null, null))
            .ReturnsAsync(sessionHistory);

        // Act
        var result = await _controller.GetSessionHistory(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedHistory = Assert.IsType<SessionHistoryDto>(okResult.Value);
        Assert.Null(returnedHistory.DateRange);
    }

}