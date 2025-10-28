using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Po.LearnCert.Api.Services;
using Po.LearnCert.Api.Repositories;
using Po.LearnCert.Api.Entities;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.UnitTests.Services;

public class UserStatisticsServiceTests
{
    private readonly Mock<IUserStatisticsRepository> _mockStatisticsRepo;
    private readonly Mock<IQuizSessionRepository> _mockSessionRepo;
    private readonly Mock<ICertificationRepository> _mockCertificationRepo;
    private readonly Mock<ISubtopicRepository> _mockSubtopicRepo;
    private readonly UserStatisticsService _service;

    public UserStatisticsServiceTests()
    {
        _mockStatisticsRepo = new Mock<IUserStatisticsRepository>();
        _mockSessionRepo = new Mock<IQuizSessionRepository>();
        _mockCertificationRepo = new Mock<ICertificationRepository>();
        _mockSubtopicRepo = new Mock<ISubtopicRepository>();

        _service = new UserStatisticsService(
            _mockStatisticsRepo.Object,
            _mockSessionRepo.Object,
            _mockCertificationRepo.Object,
            _mockSubtopicRepo.Object);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithExistingData_ReturnsCorrectStatistics()
    {
        // Arrange
        var userId = "test-user";
        var userStats = new UserStatisticsEntity
        {
            PartitionKey = userId,
            RowKey = "OVERALL",
            UserId = userId,
            TotalSessions = 5,
            TotalQuestionsAnswered = 50,
            TotalCorrectAnswers = 40,
            OverallAccuracy = 80.0,
            AverageScore = 75.5,
            BestScore = 90.0,
            LastSessionDate = DateTime.UtcNow.ToString("O"),
            TotalStudyTimeMinutes = 120
        };

        var certPerformance = new List<CertificationPerformanceEntity>
        {
            new()
            {
                PartitionKey = userId,
                RowKey = "CERT_AZ-900",
                CertificationId = "AZ-900",
                CertificationName = "Azure Fundamentals",
                SessionCount = 3,
                AverageScore = 80.0,
                BestScore = 90.0,
                QuestionsAnswered = 30,
                CorrectAnswers = 24,
                Accuracy = 80.0
            }
        };

        _mockStatisticsRepo.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync(userStats);
        _mockStatisticsRepo.Setup(x => x.GetCertificationPerformanceAsync(userId))
            .ReturnsAsync(certPerformance);

        // Act
        var result = await _service.GetUserStatisticsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(5, result.TotalSessions);
        Assert.Equal(50, result.TotalQuestionsAnswered);
        Assert.Equal(40, result.TotalCorrectAnswers);
        Assert.Equal(80.0m, result.OverallAccuracy);
        Assert.Equal(75.5m, result.AverageScore);
        Assert.Equal(90.0m, result.BestScore);
        Assert.Equal(TimeSpan.FromMinutes(120), result.TotalStudyTime);

        Assert.Single(result.CertificationPerformance);
        var certStats = result.CertificationPerformance.First();
        Assert.Equal("AZ-900", certStats.CertificationId);
        Assert.Equal("Azure Fundamentals", certStats.CertificationName);
        Assert.Equal(3, certStats.SessionCount);
        Assert.Equal(80.0m, certStats.AverageScore);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithNoData_ReturnsEmptyStatistics()
    {
        // Arrange
        var userId = "new-user";

        _mockStatisticsRepo.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync((UserStatisticsEntity?)null);
        _mockStatisticsRepo.Setup(x => x.GetCertificationPerformanceAsync(userId))
            .ReturnsAsync(new List<CertificationPerformanceEntity>());

        // Act
        var result = await _service.GetUserStatisticsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(0, result.TotalSessions);
        Assert.Equal(0, result.TotalQuestionsAnswered);
        Assert.Equal(0, result.TotalCorrectAnswers);
        Assert.Equal(0m, result.OverallAccuracy);
        Assert.Equal(0m, result.AverageScore);
        Assert.Equal(0m, result.BestScore);
        Assert.Null(result.LastSessionDate);
        Assert.Equal(TimeSpan.Zero, result.TotalStudyTime);
        Assert.Empty(result.CertificationPerformance);
    }

    [Fact]
    public async Task GetSubtopicPerformanceAsync_WithCertificationFilter_ReturnsFilteredResults()
    {
        // Arrange
        var userId = "test-user";
        var certificationId = "AZ-900";
        var subtopicPerformance = new List<SubtopicPerformanceEntity>
        {
            new()
            {
                PartitionKey = userId,
                RowKey = "SUBTOPIC_cloud-concepts",
                SubtopicId = "cloud-concepts",
                SubtopicName = "Cloud Concepts",
                CertificationId = certificationId,
                CertificationName = "Azure Fundamentals",
                QuestionsAnswered = 10,
                CorrectAnswers = 8,
                Accuracy = 80.0,
                PerformanceLevel = 2, // Good
                PracticeCount = 3,
                LastPracticedDate = DateTime.UtcNow.ToString("O"),
                AverageTimePerQuestionSeconds = 30
            }
        };

        _mockStatisticsRepo.Setup(x => x.GetSubtopicPerformanceAsync(userId, certificationId))
            .ReturnsAsync(subtopicPerformance);

        // Act
        var result = await _service.GetSubtopicPerformanceAsync(userId, certificationId);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);

        var subtopic = resultList.First();
        Assert.Equal("cloud-concepts", subtopic.SubtopicId);
        Assert.Equal("Cloud Concepts", subtopic.SubtopicName);
        Assert.Equal(certificationId, subtopic.CertificationId);
        Assert.Equal(10, subtopic.QuestionsAnswered);
        Assert.Equal(8, subtopic.CorrectAnswers);
        Assert.Equal(80.0m, subtopic.Accuracy);
        Assert.Equal(PerformanceLevel.Good, subtopic.PerformanceLevel);
        Assert.Equal(3, subtopic.PracticeCount);
        Assert.Equal(TimeSpan.FromSeconds(30), subtopic.AverageTimePerQuestion);
    }

    [Theory]
    [InlineData(95.0, PerformanceLevel.Mastered)]
    [InlineData(90.0, PerformanceLevel.Excellent)]
    [InlineData(80.0, PerformanceLevel.Good)]
    [InlineData(60.0, PerformanceLevel.Basic)]
    [InlineData(40.0, PerformanceLevel.NeedsImprovement)]
    public void CalculatePerformanceLevel_ReturnsCorrectLevel(decimal accuracy, PerformanceLevel expected)
    {
        // Act
        var result = _service.CalculatePerformanceLevel(accuracy);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task UpdateStatisticsAfterSessionAsync_WithCompletedSession_UpdatesAllStatistics()
    {
        // Arrange
        var userId = "test-user";
        var sessionId = "session-123";
        var certificationId = "AZ-900";

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            SessionId = sessionId,
            UserId = userId,
            CertificationId = certificationId,
            QuestionIds = "q1,q2,q3,q4,q5",
            CorrectAnswers = 4,
            IncorrectAnswers = 1,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            CompletedAt = DateTimeOffset.UtcNow,
            IsCompleted = true
        };

        var certification = new CertificationEntity
        {
            PartitionKey = "certifications",
            RowKey = certificationId,
            CertificationId = certificationId,
            Name = "Azure Fundamentals"
        };

        _mockSessionRepo.Setup(x => x.GetSessionByIdAsync(userId, sessionId, default))
            .ReturnsAsync(session);
        _mockCertificationRepo.Setup(x => x.GetCertificationByIdAsync(certificationId, default))
            .ReturnsAsync(certification);

        var userStats = new UserStatisticsEntity
        {
            PartitionKey = userId,
            RowKey = "OVERALL",
            UserId = userId,
            TotalSessions = 0,
            TotalQuestionsAnswered = 0,
            TotalCorrectAnswers = 0
        };

        _mockStatisticsRepo.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync(userStats);
        _mockStatisticsRepo.Setup(x => x.GetCertificationPerformanceAsync(userId, certificationId))
            .ReturnsAsync((CertificationPerformanceEntity?)null);

        // Act
        await _service.UpdateStatisticsAfterSessionAsync(userId, sessionId);

        // Assert
        _mockStatisticsRepo.Verify(x => x.UpdateUserStatisticsAsync(It.IsAny<UserStatisticsEntity>()), Times.Once);
        _mockStatisticsRepo.Verify(x => x.UpdateCertificationPerformanceAsync(It.IsAny<CertificationPerformanceEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatisticsAfterSessionAsync_WithIncompleteSession_DoesNotUpdate()
    {
        // Arrange
        var userId = "test-user";
        var sessionId = "session-123";

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            SessionId = sessionId,
            UserId = userId,
            CertificationId = "AZ-900",
            IsCompleted = false,
            CompletedAt = null
        };

        _mockSessionRepo.Setup(x => x.GetSessionByIdAsync(userId, sessionId, default))
            .ReturnsAsync(session);

        // Act
        await _service.UpdateStatisticsAfterSessionAsync(userId, sessionId);

        // Assert
        _mockStatisticsRepo.Verify(x => x.UpdateUserStatisticsAsync(It.IsAny<UserStatisticsEntity>()), Times.Never);
        _mockStatisticsRepo.Verify(x => x.UpdateCertificationPerformanceAsync(It.IsAny<CertificationPerformanceEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetSessionHistoryAsync_WithDateRange_ReturnsFilteredSessions()
    {
        // Arrange
        var userId = "test-user";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var dateRange = new DateRange { StartDate = startDate, EndDate = endDate };

        var sessions = new List<QuizSessionEntity>
        {
            new()
            {
                SessionId = "session-1",
                CertificationId = "AZ-900",
                StartedAt = DateTimeOffset.UtcNow.AddDays(-5),
                CompletedAt = DateTimeOffset.UtcNow.AddDays(-5).AddMinutes(10),
                QuestionIds = "q1,q2,q3,q4,q5",
                CorrectAnswers = 4,
                IncorrectAnswers = 1
            },
            new()
            {
                SessionId = "session-2",
                CertificationId = "Security+",
                StartedAt = DateTimeOffset.UtcNow.AddDays(-10), // Outside range
                CompletedAt = DateTimeOffset.UtcNow.AddDays(-10).AddMinutes(15),
                QuestionIds = "q6,q7,q8",
                CorrectAnswers = 2,
                IncorrectAnswers = 1
            }
        };

        var certification = new CertificationEntity
        {
            CertificationId = "AZ-900",
            Name = "Azure Fundamentals"
        };

        _mockSessionRepo.Setup(x => x.GetUserSessionsAsync(userId, default))
            .ReturnsAsync(sessions);
        _mockCertificationRepo.Setup(x => x.GetCertificationByIdAsync("AZ-900", default))
            .ReturnsAsync(certification);

        // Act
        var result = await _service.GetSessionHistoryAsync(userId, dateRange);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Sessions); // Only session-1 should be in range

        var session = result.Sessions.First();
        Assert.Equal("session-1", session.SessionId);
        Assert.Equal("Azure Fundamentals", session.CertificationName);
        Assert.Equal(5, session.TotalQuestions);
        Assert.Equal(5, session.QuestionsAnswered);
        Assert.Equal(4, session.CorrectAnswers);
        Assert.Equal(80.0m, session.ScorePercentage);
        Assert.Equal(SessionStatus.Completed, session.Status);
    }

    [Fact]
    public async Task GetSessionHistoryAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var userId = "test-user";
        var limit = 2;

        var sessions = new List<QuizSessionEntity>
        {
            new() { SessionId = "session-1", CertificationId = "AZ-900", StartedAt = DateTimeOffset.UtcNow.AddDays(-1), QuestionIds = "q1,q2", CorrectAnswers = 2, IncorrectAnswers = 0 },
            new() { SessionId = "session-2", CertificationId = "AZ-900", StartedAt = DateTimeOffset.UtcNow.AddDays(-2), QuestionIds = "q3,q4", CorrectAnswers = 1, IncorrectAnswers = 1 },
            new() { SessionId = "session-3", CertificationId = "AZ-900", StartedAt = DateTimeOffset.UtcNow.AddDays(-3), QuestionIds = "q5,q6", CorrectAnswers = 2, IncorrectAnswers = 0 }
        };

        var certification = new CertificationEntity { CertificationId = "AZ-900", Name = "Azure Fundamentals" };

        _mockSessionRepo.Setup(x => x.GetUserSessionsAsync(userId, default))
            .ReturnsAsync(sessions);
        _mockCertificationRepo.Setup(x => x.GetCertificationByIdAsync("AZ-900", default))
            .ReturnsAsync(certification);

        // Act
        var result = await _service.GetSessionHistoryAsync(userId, null, limit);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Sessions.Count); // Should be limited to 2
        Assert.Equal(2, result.TotalSessions);

        // Should be ordered by most recent first
        Assert.Equal("session-1", result.Sessions.First().SessionId);
        Assert.Equal("session-2", result.Sessions.Last().SessionId);
    }
}