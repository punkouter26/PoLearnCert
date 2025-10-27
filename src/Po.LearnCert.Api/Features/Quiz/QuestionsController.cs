using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Quiz;

/// <summary>
/// Controller for question operations.
/// </summary>
[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(
        IQuestionRepository questionRepository,
        ILogger<QuestionsController> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets a question by ID with its answer choices.
    /// </summary>
    /// <param name="certificationId">The certification ID (partition key).</param>
    /// <param name="id">The question ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The question details with answer choices.</returns>
    [HttpGet("{certificationId}/{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionDto>> GetQuestion(
        string certificationId,
        string id,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(certificationId, id, cancellationToken);
        
        if (question == null)
        {
            return NotFound(new { message = $"Question {id} not found." });
        }

        var choices = await _questionRepository.GetAnswerChoicesAsync(id, cancellationToken);

        return Ok(new QuestionDto
        {
            Id = question.RowKey,
            SubtopicId = question.SubtopicId,
            Text = question.Text,
            Explanation = question.Explanation,
            Choices = choices.Select(c => new AnswerChoiceDto
            {
                Id = c.RowKey,
                QuestionId = c.QuestionId,
                Text = c.Text,
                IsCorrect = c.IsCorrect
            }).ToList()
        });
    }
}
