using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Po.LearnCert.Api.Features.Quiz.Services;
using Po.LearnCert.Shared.Contracts;
using Po.LearnCert.Shared.Models;
using System.Security.Claims;

namespace Po.LearnCert.Api.Features.Quiz;

/// <summary>
/// Controller for quiz session operations.
/// </summary>
[ApiController]
[Route("api/quiz")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizSessionService _quizSessionService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(
        IQuizSessionService quizSessionService,
        ILogger<QuizController> logger)
    {
        _quizSessionService = quizSessionService;
        _logger = logger;
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
        return userId;
    }

    /// <summary>
    /// Creates a new quiz session.
    /// </summary>
    /// <param name="request">The quiz session creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created quiz session.</returns>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(QuizSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<QuizSessionDto>> CreateSession(
        [FromBody] CreateQuizSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var session = await _quizSessionService.CreateSessionAsync(userId, request, cancellationToken);

        return CreatedAtAction(
            nameof(GetSession),
            new { id = session.Id },
            session);
    }

    /// <summary>
    /// Submits an answer for a quiz question.
    /// </summary>
    /// <param name="id">The session ID.</param>
    /// <param name="request">The answer submission request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The answer submission response with feedback.</returns>
    [HttpPost("sessions/{id}/answers")]
    [ProducesResponseType(typeof(SubmitAnswerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer(
        string id,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.SessionId != id)
        {
            return BadRequest("Session ID in URL does not match request body.");
        }

        var userId = GetUserId();
        var response = await _quizSessionService.SubmitAnswerAsync(userId, request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets the details of a quiz session.
    /// </summary>
    /// <param name="id">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quiz session details.</returns>
    [HttpGet("sessions/{id}")]
    [ProducesResponseType(typeof(QuizSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizSessionDto>> GetSession(
        string id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var session = await _quizSessionService.GetSessionDetailsAsync(userId, id, cancellationToken);

        return Ok(session);
    }

    /// <summary>
    /// Gets the final results for a completed quiz session.
    /// </summary>
    /// <param name="id">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quiz results.</returns>
    [HttpGet("sessions/{id}/results")]
    [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResultDto>> GetSessionResults(
        string id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var results = await _quizSessionService.GetSessionResultsAsync(userId, id, cancellationToken);

        return Ok(results);
    }
}
