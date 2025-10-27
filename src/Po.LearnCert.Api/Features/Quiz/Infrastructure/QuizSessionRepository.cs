using Azure;
using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Repository for quiz session operations.
/// </summary>
public class QuizSessionRepository : IQuizSessionRepository
{
    private readonly TableClient _sessionsTable;
    private readonly TableClient _answersTable;
    private readonly ILogger<QuizSessionRepository> _logger;

    public QuizSessionRepository(TableServiceClient tableServiceClient, ILogger<QuizSessionRepository> logger)
    {
        _logger = logger;
        _sessionsTable = tableServiceClient.GetTableClient("PoLearnCertQuizSessions");
        _answersTable = tableServiceClient.GetTableClient("PoLearnCertSessionAnswers");
        
        _sessionsTable.CreateIfNotExists();
        _answersTable.CreateIfNotExists();
    }

    public async Task<QuizSessionEntity> CreateSessionAsync(QuizSessionEntity session, CancellationToken cancellationToken = default)
    {
        await _sessionsTable.AddEntityAsync(session, cancellationToken);
        _logger.LogInformation("Created quiz session: {SessionId} for user {UserId}", session.SessionId, session.UserId);
        return session;
    }

    public async Task<QuizSessionEntity?> GetSessionByIdAsync(string userId, string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _sessionsTable.GetEntityAsync<QuizSessionEntity>(userId, sessionId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Session not found: {UserId}/{SessionId}", userId, sessionId);
            return null;
        }
    }

    public async Task<QuizSessionEntity> UpdateSessionAsync(QuizSessionEntity session, CancellationToken cancellationToken = default)
    {
        await _sessionsTable.UpdateEntityAsync(session, session.ETag, TableUpdateMode.Replace, cancellationToken);
        _logger.LogInformation("Updated quiz session: {SessionId}", session.SessionId);
        return session;
    }

    public async Task<IEnumerable<QuizSessionEntity>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var sessions = new List<QuizSessionEntity>();
        var filter = $"PartitionKey eq '{userId}'";
        
        await foreach (var session in _sessionsTable.QueryAsync<QuizSessionEntity>(filter, cancellationToken: cancellationToken))
        {
            sessions.Add(session);
        }
        
        _logger.LogDebug("Retrieved {Count} sessions for user {UserId}", sessions.Count, userId);
        return sessions.OrderByDescending(s => s.StartedAt).ToList();
    }

    public async Task<SessionAnswerEntity> RecordAnswerAsync(SessionAnswerEntity answer, CancellationToken cancellationToken = default)
    {
        await _answersTable.UpsertEntityAsync(answer, TableUpdateMode.Replace, cancellationToken);
        _logger.LogInformation("Recorded answer for session {SessionId}, question {QuestionId}", 
            answer.SessionId, answer.QuestionId);
        return answer;
    }

    public async Task<IEnumerable<SessionAnswerEntity>> GetSessionAnswersAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var answers = new List<SessionAnswerEntity>();
        var filter = $"PartitionKey eq '{sessionId}'";
        
        await foreach (var answer in _answersTable.QueryAsync<SessionAnswerEntity>(filter, cancellationToken: cancellationToken))
        {
            answers.Add(answer);
        }
        
        _logger.LogDebug("Retrieved {Count} answers for session {SessionId}", answers.Count, sessionId);
        return answers;
    }

    public async Task<SessionAnswerEntity?> GetAnswerAsync(string sessionId, string questionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _answersTable.GetEntityAsync<SessionAnswerEntity>(sessionId, questionId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
