using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Repository for question operations.
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly TableClient _questionsTable;
    private readonly TableClient _choicesTable;
    private readonly ILogger<QuestionRepository> _logger;

    public QuestionRepository(TableServiceClient tableServiceClient, ILogger<QuestionRepository> logger)
    {
        _logger = logger;
        _questionsTable = tableServiceClient.GetTableClient("PoLearnCertQuestions");
        _choicesTable = tableServiceClient.GetTableClient("PoLearnCertAnswerChoices");

        _questionsTable.CreateIfNotExists();
        _choicesTable.CreateIfNotExists();
    }

    public async Task<IEnumerable<QuestionEntity>> GetQuestionsBySubtopicAsync(string subtopicId, CancellationToken cancellationToken = default)
    {
        var questions = new List<QuestionEntity>();
        var filter = $"PartitionKey eq '{subtopicId}'";

        await foreach (var question in _questionsTable.QueryAsync<QuestionEntity>(filter, cancellationToken: cancellationToken))
        {
            questions.Add(question);
        }

        _logger.LogDebug("Retrieved {Count} questions for subtopic {SubtopicId}", questions.Count, subtopicId);
        return questions;
    }

    public async Task<IEnumerable<QuestionEntity>> GetQuestionsByCertificationAsync(string certificationId, CancellationToken cancellationToken = default)
    {
        var questions = new List<QuestionEntity>();
        // Query by PartitionKey (CertificationId) for efficient table storage queries
        var filter = $"PartitionKey eq '{certificationId}'";

        await foreach (var question in _questionsTable.QueryAsync<QuestionEntity>(filter, cancellationToken: cancellationToken))
        {
            questions.Add(question);
        }

        _logger.LogDebug("Retrieved {Count} questions for certification {CertificationId}", questions.Count, certificationId);
        return questions;
    }

    public async Task<QuestionEntity?> GetQuestionByIdAsync(string certificationId, string questionId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use certificationId as PartitionKey, questionId as RowKey
            var response = await _questionsTable.GetEntityAsync<QuestionEntity>(certificationId, questionId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Question not found: {CertificationId}/{QuestionId}", certificationId, questionId);
            return null;
        }
    }

    public async Task<IEnumerable<AnswerChoiceEntity>> GetAnswerChoicesAsync(string questionId, CancellationToken cancellationToken = default)
    {
        var choices = new List<AnswerChoiceEntity>();
        var filter = $"PartitionKey eq '{questionId}'";

        await foreach (var choice in _choicesTable.QueryAsync<AnswerChoiceEntity>(filter, cancellationToken: cancellationToken))
        {
            choices.Add(choice);
        }

        _logger.LogDebug("Retrieved {Count} answer choices for question {QuestionId}", choices.Count, questionId);
        return choices.OrderBy(c => c.Order).ToList();
    }

    public async Task<IEnumerable<QuestionEntity>> GetRandomQuestionsAsync(string certificationId, string? subtopicId, int count, CancellationToken cancellationToken = default)
    {
        IEnumerable<QuestionEntity> allQuestions;

        if (!string.IsNullOrEmpty(subtopicId))
        {
            allQuestions = await GetQuestionsBySubtopicAsync(subtopicId, cancellationToken);
        }
        else
        {
            allQuestions = await GetQuestionsByCertificationAsync(certificationId, cancellationToken);
        }

        var random = new Random();
        var selectedQuestions = allQuestions.OrderBy(x => random.Next()).Take(count).ToList();

        _logger.LogInformation("Selected {Count} random questions from {TotalCount} available",
            selectedQuestions.Count, allQuestions.Count());

        return selectedQuestions;
    }
}
