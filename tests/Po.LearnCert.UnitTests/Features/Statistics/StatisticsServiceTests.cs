using Xunit;
using FluentAssertions;
using Moq;
using Po.LearnCert.Api.Features.Statistics.Services;
using Po.LearnCert.Api.Features.Statistics.Repositories;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Statistics.Entities;
using Po.LearnCert.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Po.LearnCert.UnitTests.Features.Statistics;

public class StatisticsServiceTests
{
    private readonly Mock<IUserStatisticsRepository> _mockRepository;
    private readonly Mock<IQuizSessionRepository> _mockSessionRepository;
    private readonly Mock<ICertificationRepository> _mockCertificationRepository;
    private readonly Mock<ISubtopicRepository> _mockSubtopicRepository;
    private readonly UserStatisticsService _service;

    public StatisticsServiceTests()
    {
        _mockRepository = new Mock<IUserStatisticsRepository>();
        _mockSessionRepository = new Mock<IQuizSessionRepository>();
        _mockCertificationRepository = new Mock<ICertificationRepository>();
        _mockSubtopicRepository = new Mock<ISubtopicRepository>();
        _service = new UserStatisticsService(
            _mockRepository.Object,
            _mockSessionRepository.Object,
            _mockCertificationRepository.Object,
            _mockSubtopicRepository.Object);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithNoSessions_ReturnsZeroStats()
    {
        // Arrange
        var userId = "user123";
        _mockRepository
            .Setup(r => r.GetUserStatisticsAsync(userId))
            .ReturnsAsync((UserStatisticsEntity?)null);

        _mockRepository
            .Setup(r => r.GetCertificationPerformanceAsync(userId))
            .ReturnsAsync(new List<CertificationPerformanceEntity>());

        // Act
        var result = await _service.GetUserStatisticsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalSessions.Should().Be(0);
        result.TotalQuestionsAnswered.Should().Be(0);
        result.OverallAccuracy.Should().Be(0m);
        result.AverageScore.Should().Be(0m);
        result.BestScore.Should().Be(0m);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithExistingStats_ReturnsCorrectData()
    {
        // Arrange
        var userId = "user123";
        var statsEntity = new UserStatisticsEntity
        {
            PartitionKey = userId,
            RowKey = "STATS",
            TotalSessions = 10,
            TotalQuestionsAnswered = 200,
            TotalCorrectAnswers = 150,
            OverallAccuracy = 75.0,
            AverageScore = 75.5,
            BestScore = 95.0,
            TotalStudyTimeMinutes = 300,
            LastSessionDate = DateTime.UtcNow.AddDays(-1).ToString("o")
        };

        _mockRepository
            .Setup(r => r.GetUserStatisticsAsync(userId))
            .ReturnsAsync(statsEntity);

        _mockRepository
            .Setup(r => r.GetCertificationPerformanceAsync(userId))
            .ReturnsAsync(new List<CertificationPerformanceEntity>());

        // Act
        var result = await _service.GetUserStatisticsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalSessions.Should().Be(10);
        result.TotalQuestionsAnswered.Should().Be(200);
        result.OverallAccuracy.Should().Be(75.0m);
        result.AverageScore.Should().Be(75.5m);
        result.BestScore.Should().Be(95.0m);
        result.TotalStudyTime.Should().Be(TimeSpan.FromMinutes(300));
    }

    [Fact]
    public async Task GetSubtopicPerformanceAsync_WithNoCertification_ReturnsAllSubtopics()
    {
        // Arrange
        var userId = "user123";
        var performanceData = new List<SubtopicPerformanceEntity>
        {
            new SubtopicPerformanceEntity
            {
                PartitionKey = userId,
                RowKey = "SUBTOPIC_subtopic1",
                UserId = userId,
                SubtopicId = "subtopic1",
                SubtopicName = "Cloud Concepts",
                CertificationId = "az900",
                CertificationName = "Azure Fundamentals",
                QuestionsAnswered = 5,
                CorrectAnswers = 4,
                Accuracy = 80.0,
                PerformanceLevel = 3,
                PracticeCount = 2,
                LastPracticedDate = DateTime.UtcNow.AddDays(-1).ToString("o"),
                AverageTimePerQuestionSeconds = 30
            },
            new SubtopicPerformanceEntity
            {
                PartitionKey = userId,
                RowKey = "SUBTOPIC_subtopic2",
                UserId = userId,
                SubtopicId = "subtopic2",
                SubtopicName = "Security",
                CertificationId = "secplus",
                CertificationName = "Security+",
                QuestionsAnswered = 3,
                CorrectAnswers = 2,
                Accuracy = 66.7,
                PerformanceLevel = 2,
                PracticeCount = 1,
                LastPracticedDate = DateTime.UtcNow.AddDays(-2).ToString("o"),
                AverageTimePerQuestionSeconds = 45
            }
        };

        _mockRepository
            .Setup(r => r.GetSubtopicPerformanceAsync(userId))
            .ReturnsAsync(performanceData);

        // Act
        var result = await _service.GetSubtopicPerformanceAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        resultList[0].SubtopicName.Should().Be("Cloud Concepts");
        resultList[1].SubtopicName.Should().Be("Security");
    }

    [Fact]
    public async Task GetSubtopicPerformanceAsync_WithCertificationFilter_ReturnsFilteredResults()
    {
        // Arrange
        var userId = "user123";
        var certificationId = "az900";
        var performanceData = new List<SubtopicPerformanceEntity>
        {
            new SubtopicPerformanceEntity
            {
                PartitionKey = userId,
                RowKey = "SUBTOPIC_subtopic1",
                UserId = userId,
                SubtopicId = "subtopic1",
                SubtopicName = "Cloud Concepts",
                CertificationId = "az900",
                CertificationName = "Azure Fundamentals",
                QuestionsAnswered = 5,
                CorrectAnswers = 4,
                Accuracy = 80.0,
                PerformanceLevel = 3,
                PracticeCount = 2,
                LastPracticedDate = DateTime.UtcNow.AddDays(-1).ToString("o"),
                AverageTimePerQuestionSeconds = 30
            }
        };

        _mockRepository
            .Setup(r => r.GetSubtopicPerformanceAsync(userId, certificationId))
            .ReturnsAsync(performanceData);

        // Act
        var result = await _service.GetSubtopicPerformanceAsync(userId, certificationId);

        // Assert
        result.Should().HaveCount(1);
        var resultList = result.ToList();
        resultList[0].CertificationId.Should().Be(certificationId);
    }
}
