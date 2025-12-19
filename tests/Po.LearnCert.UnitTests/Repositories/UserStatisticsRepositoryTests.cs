using Xunit;
using Moq;
using Po.LearnCert.Api.Features.Statistics.Repositories;
using Po.LearnCert.Api.Features.Statistics.Entities;
using Azure.Data.Tables;
using Azure;

namespace Po.LearnCert.UnitTests.Repositories;

public class UserStatisticsRepositoryTests
{
    private readonly Mock<TableServiceClient> _mockTableServiceClient;
    private readonly Mock<TableClient> _mockTableClient;
    private readonly UserStatisticsRepository _repository;

    public UserStatisticsRepositoryTests()
    {
        _mockTableServiceClient = new Mock<TableServiceClient>();
        _mockTableClient = new Mock<TableClient>();

        _mockTableServiceClient.Setup(x => x.GetTableClient("UserStatistics"))
            .Returns(_mockTableClient.Object);

        _repository = new UserStatisticsRepository(_mockTableServiceClient.Object);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithExistingUser_ReturnsEntity()
    {
        // Arrange
        var userId = "test-user";
        var entity = new UserStatisticsEntity
        {
            PartitionKey = userId,
            RowKey = "OVERALL",
            UserId = userId,
            TotalSessions = 5
        };

        var response = Response.FromValue(entity, Mock.Of<Response>());
        _mockTableClient.Setup(x => x.GetEntityAsync<UserStatisticsEntity>(userId, "OVERALL", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.GetUserStatisticsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(5, result.TotalSessions);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var userId = "non-existent-user";
        var exception = new RequestFailedException(404, "Not Found");

        _mockTableClient.Setup(x => x.GetEntityAsync<UserStatisticsEntity>(userId, "OVERALL", null, default))
            .ThrowsAsync(exception);

        // Act
        var result = await _repository.GetUserStatisticsAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserStatisticsAsync_UpdatesLastUpdatedTime()
    {
        // Arrange
        var entity = new UserStatisticsEntity
        {
            PartitionKey = "test-user",
            RowKey = "OVERALL",
            UserId = "test-user",
            TotalSessions = 5
        };

        var originalTime = entity.LastUpdated;
        await Task.Delay(10); // Ensure time difference

        var response = Mock.Of<Response>();
        _mockTableClient.Setup(x => x.UpsertEntityAsync(entity, TableUpdateMode.Merge, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.UpdateUserStatisticsAsync(entity);

        // Assert
        Assert.True(result.LastUpdated > originalTime);
        _mockTableClient.Verify(x => x.UpsertEntityAsync(entity, TableUpdateMode.Merge, default), Times.Once);
    }

    [Fact]
    public async Task GetCertificationPerformanceAsync_ReturnsFilteredEntities()
    {
        // Arrange
        var userId = "test-user";
        var entities = new List<CertificationPerformanceEntity>
        {
            new()
            {
                PartitionKey = userId,
                RowKey = "CERT_AZ-900",
                CertificationId = "AZ-900",
                CertificationName = "Azure Fundamentals"
            },
            new()
            {
                PartitionKey = userId,
                RowKey = "CERT_Security+",
                CertificationId = "Security+",
                CertificationName = "CompTIA Security+"
            }
        };

        _mockTableClient.Setup(x => x.QueryAsync<CertificationPerformanceEntity>(
            It.Is<string>(filter => filter.Contains($"PartitionKey eq '{userId}'")),
            null, null, default))
            .Returns(CreateAsyncPageable(entities));

        // Act
        var result = await _repository.GetCertificationPerformanceAsync(userId);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, x => x.CertificationId == "AZ-900");
        Assert.Contains(resultList, x => x.CertificationId == "Security+");
    }

    [Fact]
    public async Task GetCertificationPerformanceAsync_WithSpecificCertification_ReturnsSpecificEntity()
    {
        // Arrange
        var userId = "test-user";
        var certificationId = "AZ-900";
        var entity = new CertificationPerformanceEntity
        {
            PartitionKey = userId,
            RowKey = $"CERT_{certificationId}",
            CertificationId = certificationId,
            CertificationName = "Azure Fundamentals"
        };

        var response = Response.FromValue(entity, Mock.Of<Response>());
        _mockTableClient.Setup(x => x.GetEntityAsync<CertificationPerformanceEntity>(userId, $"CERT_{certificationId}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.GetCertificationPerformanceAsync(userId, certificationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(certificationId, result.CertificationId);
        Assert.Equal("Azure Fundamentals", result.CertificationName);
    }

    [Fact]
    public async Task GetSubtopicPerformanceAsync_WithCertificationFilter_ReturnsFilteredEntities()
    {
        // Arrange
        var userId = "test-user";
        var certificationId = "AZ-900";
        var entities = new List<SubtopicPerformanceEntity>
        {
            new()
            {
                PartitionKey = userId,
                RowKey = "SUBTOPIC_cloud-concepts",
                SubtopicId = "cloud-concepts",
                SubtopicName = "Cloud Concepts",
                CertificationId = certificationId
            },
            new()
            {
                PartitionKey = userId,
                RowKey = "SUBTOPIC_azure-services",
                SubtopicId = "azure-services",
                SubtopicName = "Azure Services",
                CertificationId = certificationId
            }
        };

        _mockTableClient.Setup(x => x.QueryAsync<SubtopicPerformanceEntity>(
            It.Is<string>(filter => filter.Contains($"CertificationId eq '{certificationId}'")),
            null, null, default))
            .Returns(CreateAsyncPageable(entities));

        // Act
        var result = await _repository.GetSubtopicPerformanceAsync(userId, certificationId);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, x => Assert.Equal(certificationId, x.CertificationId));
    }

    [Fact]
    public async Task GetSingleSubtopicPerformanceAsync_WithExistingSubtopic_ReturnsEntity()
    {
        // Arrange
        var userId = "test-user";
        var subtopicId = "cloud-concepts";
        var entity = new SubtopicPerformanceEntity
        {
            PartitionKey = userId,
            RowKey = $"SUBTOPIC_{subtopicId}",
            SubtopicId = subtopicId,
            SubtopicName = "Cloud Concepts"
        };

        var response = Response.FromValue(entity, Mock.Of<Response>());
        _mockTableClient.Setup(x => x.GetEntityAsync<SubtopicPerformanceEntity>(userId, $"SUBTOPIC_{subtopicId}", null, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.GetSingleSubtopicPerformanceAsync(userId, subtopicId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subtopicId, result.SubtopicId);
        Assert.Equal("Cloud Concepts", result.SubtopicName);
    }

    [Fact]
    public async Task UpdateCertificationPerformanceAsync_UpdatesLastUpdatedTime()
    {
        // Arrange
        var entity = new CertificationPerformanceEntity
        {
            PartitionKey = "test-user",
            RowKey = "CERT_AZ-900",
            CertificationId = "AZ-900",
            CertificationName = "Azure Fundamentals"
        };

        var originalTime = entity.LastUpdated;
        await Task.Delay(10); // Ensure time difference

        var response = Mock.Of<Response>();
        _mockTableClient.Setup(x => x.UpsertEntityAsync(entity, TableUpdateMode.Merge, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.UpdateCertificationPerformanceAsync(entity);

        // Assert
        Assert.True(result.LastUpdated > originalTime);
        _mockTableClient.Verify(x => x.UpsertEntityAsync(entity, TableUpdateMode.Merge, default), Times.Once);
    }

    [Fact]
    public async Task UpdateSubtopicPerformanceAsync_UpdatesLastUpdatedTime()
    {
        // Arrange
        var entity = new SubtopicPerformanceEntity
        {
            PartitionKey = "test-user",
            RowKey = "SUBTOPIC_cloud-concepts",
            SubtopicId = "cloud-concepts",
            SubtopicName = "Cloud Concepts"
        };

        var originalTime = entity.LastUpdated;
        await Task.Delay(10); // Ensure time difference

        var response = Mock.Of<Response>();
        _mockTableClient.Setup(x => x.UpsertEntityAsync(entity, TableUpdateMode.Merge, default))
            .ReturnsAsync(response);

        // Act
        var result = await _repository.UpdateSubtopicPerformanceAsync(entity);

        // Assert
        Assert.True(result.LastUpdated > originalTime);
        _mockTableClient.Verify(x => x.UpsertEntityAsync(entity, TableUpdateMode.Merge, default), Times.Once);
    }

    private static AsyncPageable<T> CreateAsyncPageable<T>(IEnumerable<T> items) where T : class
    {
        var pages = new[] { Page<T>.FromValues(items.ToArray(), null, Mock.Of<Response>()) };
        return AsyncPageable<T>.FromPages(pages);
    }
}