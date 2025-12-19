using Moq;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Services;
using Po.LearnCert.Api.Features.Quiz.Services.Handlers;
using Po.LearnCert.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Po.LearnCert.UnitTests.Features.Quiz;

public class QuizSessionServiceTests
{
    private readonly Mock<IQuizSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICertificationRepository> _certificationRepositoryMock;
    private readonly Mock<IAnswerSubmissionHandler> _answerHandlerMock;
    private readonly Mock<IQuizCompletionHandler> _completionHandlerMock;
    private readonly Mock<ILogger<QuizSessionService>> _loggerMock;
    private readonly QuizSessionService _service;

    public QuizSessionServiceTests()
    {
        _sessionRepositoryMock = new Mock<IQuizSessionRepository>();
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _certificationRepositoryMock = new Mock<ICertificationRepository>();
        _answerHandlerMock = new Mock<IAnswerSubmissionHandler>();
        _completionHandlerMock = new Mock<IQuizCompletionHandler>();
        _loggerMock = new Mock<ILogger<QuizSessionService>>();

        _service = new QuizSessionService(
            _sessionRepositoryMock.Object,
            _questionRepositoryMock.Object,
            _certificationRepositoryMock.Object,
            _answerHandlerMock.Object,
            _completionHandlerMock.Object,
            _loggerMock.Object);
    }

    #region CreateSessionAsync Tests

    [Fact]
    public async Task CreateSessionAsync_WithValidCertification_ReturnsQuizSession()
    {
        // Arrange
        var userId = "user1";
        var certificationId = "cert1";
        var request = new CreateQuizSessionRequest
        {
            CertificationId = certificationId,
            QuestionCount = 5
        };

        var certification = new CertificationEntity
        {
            PartitionKey = "Certifications",
            RowKey = certificationId,
            Name = "Test Certification"
        };

        var questions = new List<QuestionEntity>
        {
            new() { PartitionKey = certificationId, RowKey = "q1", Text = "Q1" },
            new() { PartitionKey = certificationId, RowKey = "q2", Text = "Q2" },
            new() { PartitionKey = certificationId, RowKey = "q3", Text = "Q3" }
        };

        var createdSession = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = "session1",
            UserId = userId,
            CertificationId = certificationId,
            QuestionIds = "q1,q2,q3",
            CurrentQuestionIndex = 0,
            IsCompleted = false,
            StartedAt = DateTimeOffset.UtcNow
        };

        _certificationRepositoryMock
            .Setup(x => x.GetCertificationByIdAsync(certificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(certification);

        _questionRepositoryMock
            .Setup(x => x.GetRandomQuestionsAsync(certificationId, null, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        _sessionRepositoryMock
            .Setup(x => x.CreateSessionAsync(It.IsAny<QuizSessionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act
        var result = await _service.CreateSessionAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(certificationId, result.CertificationId);
        Assert.Equal(3, result.QuestionIds.Count);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task CreateSessionAsync_WithInvalidCertification_ThrowsException()
    {
        // Arrange
        var userId = "user1";
        var request = new CreateQuizSessionRequest
        {
            CertificationId = "invalid-cert",
            QuestionCount = 5
        };

        _certificationRepositoryMock
            .Setup(x => x.GetCertificationByIdAsync("invalid-cert", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CertificationEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateSessionAsync(userId, request));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateSessionAsync_WithNoAvailableQuestions_ThrowsException()
    {
        // Arrange
        var userId = "user1";
        var certificationId = "cert1";
        var request = new CreateQuizSessionRequest
        {
            CertificationId = certificationId,
            QuestionCount = 5
        };

        var certification = new CertificationEntity
        {
            PartitionKey = "Certifications",
            RowKey = certificationId,
            Name = "Test Certification"
        };

        _certificationRepositoryMock
            .Setup(x => x.GetCertificationByIdAsync(certificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(certification);

        _questionRepositoryMock
            .Setup(x => x.GetRandomQuestionsAsync(certificationId, null, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QuestionEntity>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateSessionAsync(userId, request));
        Assert.Contains("No questions available", exception.Message);
    }

    [Fact]
    public async Task CreateSessionAsync_WithSubtopic_FiltersBySubtopic()
    {
        // Arrange
        var userId = "user1";
        var certificationId = "cert1";
        var subtopicId = "subtopic1";
        var request = new CreateQuizSessionRequest
        {
            CertificationId = certificationId,
            SubtopicId = subtopicId,
            QuestionCount = 5
        };

        var certification = new CertificationEntity
        {
            PartitionKey = "Certifications",
            RowKey = certificationId,
            Name = "Test Certification"
        };

        var subtopic = new SubtopicEntity
        {
            PartitionKey = certificationId,
            RowKey = subtopicId,
            Name = "Test Subtopic"
        };

        var questions = new List<QuestionEntity>
        {
            new() { PartitionKey = certificationId, RowKey = "q1", Text = "Q1" }
        };

        var createdSession = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = "session1",
            UserId = userId,
            CertificationId = certificationId,
            SubtopicId = subtopicId,
            QuestionIds = "q1",
            CurrentQuestionIndex = 0,
            IsCompleted = false,
            StartedAt = DateTimeOffset.UtcNow
        };

        _certificationRepositoryMock
            .Setup(x => x.GetCertificationByIdAsync(certificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(certification);

        _certificationRepositoryMock
            .Setup(x => x.GetSubtopicByIdAsync(certificationId, subtopicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subtopic);

        _questionRepositoryMock
            .Setup(x => x.GetRandomQuestionsAsync(certificationId, subtopicId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        _sessionRepositoryMock
            .Setup(x => x.CreateSessionAsync(It.IsAny<QuizSessionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession);

        // Act
        var result = await _service.CreateSessionAsync(userId, request);

        // Assert
        Assert.Equal(subtopicId, result.SubtopicId);
        _questionRepositoryMock.Verify(
            x => x.GetRandomQuestionsAsync(certificationId, subtopicId, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SubmitAnswerAsync Tests

    [Fact]
    public async Task SubmitAnswerAsync_WithCorrectAnswer_ReturnsCorrectResponse()
    {
        // Arrange
        var userId = "user1";
        var sessionId = "session1";
        var questionId = "q1";
        var choiceId = "choice1";

        var request = new SubmitAnswerRequest
        {
            SessionId = sessionId,
            QuestionId = questionId,
            SelectedChoiceId = choiceId
        };

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            CertificationId = "cert1",
            QuestionIds = "q1,q2,q3",
            CurrentQuestionIndex = 0,
            CorrectAnswers = 0,
            IncorrectAnswers = 0,
            IsCompleted = false
        };

        var question = new QuestionEntity
        {
            PartitionKey = "cert1",
            RowKey = questionId,
            Text = "Test Question",
            Explanation = "Test explanation"
        };

        var selectedChoice = new AnswerChoiceEntity
        {
            PartitionKey = questionId,
            RowKey = choiceId,
            IsCorrect = true
        };

        var correctChoice = selectedChoice;

        var validationResult = new AnswerValidationResult(
            true, null, session, question, selectedChoice, correctChoice);

        var updatedSession = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            CertificationId = "cert1",
            QuestionIds = "q1,q2,q3",
            CurrentQuestionIndex = 1,
            CorrectAnswers = 1,
            IncorrectAnswers = 0,
            IsCompleted = false
        };

        _answerHandlerMock
            .Setup(x => x.ValidateAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _answerHandlerMock
            .Setup(x => x.RecordAnswerAsync(session, request, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, updatedSession));

        // Act
        var result = await _service.SubmitAnswerAsync(userId, request);

        // Assert
        Assert.True(result.IsCorrect);
        Assert.Equal(choiceId, result.CorrectChoiceId);
        Assert.Equal("Test explanation", result.Explanation);
        Assert.Equal(1, result.CorrectAnswers);
        Assert.Equal(0, result.IncorrectAnswers);
        Assert.False(result.IsSessionComplete);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WithInvalidSession_ThrowsException()
    {
        // Arrange
        var userId = "user1";
        var request = new SubmitAnswerRequest
        {
            SessionId = "invalid-session",
            QuestionId = "q1",
            SelectedChoiceId = "choice1"
        };

        var validationResult = new AnswerValidationResult(
            false, "Session invalid-session not found for user user1.");

        _answerHandlerMock
            .Setup(x => x.ValidateAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitAnswerAsync(userId, request));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WhenSessionCompletes_ProcessesCompletion()
    {
        // Arrange
        var userId = "user1";
        var sessionId = "session1";
        var request = new SubmitAnswerRequest
        {
            SessionId = sessionId,
            QuestionId = "q3",
            SelectedChoiceId = "choice1"
        };

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            CertificationId = "cert1",
            QuestionIds = "q1,q2,q3",
            CurrentQuestionIndex = 2,
            CorrectAnswers = 2,
            IncorrectAnswers = 0,
            IsCompleted = false
        };

        var question = new QuestionEntity { PartitionKey = "cert1", RowKey = "q3", Explanation = "Done" };
        var choice = new AnswerChoiceEntity { PartitionKey = "q3", RowKey = "choice1", IsCorrect = true };

        var validationResult = new AnswerValidationResult(true, null, session, question, choice, choice);

        var completedSession = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            CertificationId = "cert1",
            QuestionIds = "q1,q2,q3",
            CurrentQuestionIndex = 3,
            CorrectAnswers = 3,
            IncorrectAnswers = 0,
            IsCompleted = true,
            CompletedAt = DateTimeOffset.UtcNow
        };

        _answerHandlerMock
            .Setup(x => x.ValidateAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _answerHandlerMock
            .Setup(x => x.RecordAnswerAsync(session, request, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, completedSession));

        // Act
        var result = await _service.SubmitAnswerAsync(userId, request);

        // Assert
        Assert.True(result.IsSessionComplete);
        _completionHandlerMock.Verify(
            x => x.ProcessCompletionAsync(userId, completedSession, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetSessionResultsAsync Tests

    [Fact]
    public async Task GetSessionResultsAsync_WithCompletedSession_ReturnsResults()
    {
        // Arrange
        var userId = "user1";
        var sessionId = "session1";
        var certificationId = "cert1";

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            CertificationId = certificationId,
            QuestionIds = "q1,q2,q3",
            CorrectAnswers = 2,
            IncorrectAnswers = 1,
            IsCompleted = true,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            CompletedAt = DateTimeOffset.UtcNow
        };

        var certification = new CertificationEntity
        {
            PartitionKey = "Certifications",
            RowKey = certificationId,
            Name = "Test Certification"
        };

        _sessionRepositoryMock
            .Setup(x => x.GetSessionByIdAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _certificationRepositoryMock
            .Setup(x => x.GetCertificationByIdAsync(certificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(certification);

        // Act
        var result = await _service.GetSessionResultsAsync(userId, sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal("Test Certification", result.CertificationName);
        Assert.Equal(2, result.CorrectAnswers);
        Assert.Equal(1, result.IncorrectAnswers);
        Assert.Equal(3, result.TotalQuestions);
        Assert.True(result.DurationSeconds > 0);
    }

    [Fact]
    public async Task GetSessionResultsAsync_WithIncompleteSession_ThrowsException()
    {
        // Arrange
        var userId = "user1";
        var sessionId = "session1";

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            IsCompleted = false
        };

        _sessionRepositoryMock
            .Setup(x => x.GetSessionByIdAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetSessionResultsAsync(userId, sessionId));
        Assert.Contains("not completed", exception.Message);
    }

    [Fact]
    public async Task GetSessionResultsAsync_WithNonExistentSession_ThrowsException()
    {
        // Arrange
        var userId = "user1";
        var sessionId = "invalid-session";

        _sessionRepositoryMock
            .Setup(x => x.GetSessionByIdAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuizSessionEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetSessionResultsAsync(userId, sessionId));
        Assert.Contains("not found", exception.Message);
    }

    #endregion

    #region GetSessionDetailsAsync Tests

    [Fact]
    public async Task GetSessionDetailsAsync_WithValidSession_ReturnsDetails()
    {
        // Arrange
        var userId = "user1";
        var sessionId = "session1";

        var session = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = sessionId,
            UserId = userId,
            CertificationId = "cert1",
            QuestionIds = "q1,q2,q3",
            CurrentQuestionIndex = 1,
            CorrectAnswers = 1,
            IncorrectAnswers = 0,
            IsCompleted = false,
            StartedAt = DateTimeOffset.UtcNow
        };

        _sessionRepositoryMock
            .Setup(x => x.GetSessionByIdAsync(userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _service.GetSessionDetailsAsync(userId, sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("cert1", result.CertificationId);
        Assert.Equal(3, result.QuestionIds.Count);
        Assert.Equal(1, result.CurrentQuestionIndex);
        Assert.False(result.IsCompleted);
    }

    #endregion
}
