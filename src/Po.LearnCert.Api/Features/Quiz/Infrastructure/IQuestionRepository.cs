namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Repository interface for question operations.
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// Gets questions by subtopic.
    /// </summary>
    Task<IEnumerable<QuestionEntity>> GetQuestionsBySubtopicAsync(string subtopicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets questions by certification (across all subtopics).
    /// </summary>
    Task<IEnumerable<QuestionEntity>> GetQuestionsByCertificationAsync(string certificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a question by ID.
    /// </summary>
    Task<QuestionEntity?> GetQuestionByIdAsync(string certificationId, string questionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets answer choices for a question.
    /// </summary>
    Task<IEnumerable<AnswerChoiceEntity>> GetAnswerChoicesAsync(string questionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets random questions for a quiz session.
    /// </summary>
    Task<IEnumerable<QuestionEntity>> GetRandomQuestionsAsync(string certificationId, string? subtopicId, int count, CancellationToken cancellationToken = default);
}
