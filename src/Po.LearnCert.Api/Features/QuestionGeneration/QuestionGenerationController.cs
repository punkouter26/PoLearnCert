using Microsoft.AspNetCore.Mvc;

namespace Po.LearnCert.Api.Features.QuestionGeneration;

/// <summary>
/// Controller for AI-powered question generation.
/// </summary>
[ApiController]
[Route("api/questions/generate")]
public class QuestionGenerationController : ControllerBase
{
    private readonly IQuestionGenerationService _generationService;
    private readonly ILogger<QuestionGenerationController> _logger;

    public QuestionGenerationController(
        IQuestionGenerationService generationService,
        ILogger<QuestionGenerationController> logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>
    /// Generates questions for a specific subtopic.
    /// </summary>
    [HttpPost("subtopic")]
    [ProducesResponseType(typeof(GenerationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerationResultDto>> GenerateForSubtopic(
        [FromBody] GenerateForSubtopicRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Count <= 0 || request.Count > 100)
        {
            return BadRequest("Count must be between 1 and 100");
        }

        _logger.LogInformation("Generating {Count} questions for subtopic {SubtopicId}",
            request.Count, request.SubtopicId);

        var result = await _generationService.GenerateQuestionsAsync(
            request.CertificationId,
            request.SubtopicId,
            request.SubtopicName,
            request.Count,
            cancellationToken: cancellationToken);

        return Ok(new GenerationResultDto(
            result.TotalGenerated,
            result.TotalFailed,
            result.Duration.TotalSeconds,
            result.Errors.Take(10).ToList()));
    }

    /// <summary>
    /// Generates questions for all subtopics of a certification.
    /// </summary>
    [HttpPost("certification")]
    [ProducesResponseType(typeof(GenerationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerationResultDto>> GenerateForCertification(
        [FromBody] GenerateForCertificationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.QuestionsPerSubtopic <= 0 || request.QuestionsPerSubtopic > 100)
        {
            return BadRequest("QuestionsPerSubtopic must be between 1 and 100");
        }

        _logger.LogInformation("Generating {Count} questions per subtopic for certification {CertificationId}",
            request.QuestionsPerSubtopic, request.CertificationId);

        var result = await _generationService.GenerateQuestionsForCertificationAsync(
            request.CertificationId,
            request.QuestionsPerSubtopic,
            cancellationToken: cancellationToken);

        return Ok(new GenerationResultDto(
            result.TotalGenerated,
            result.TotalFailed,
            result.Duration.TotalSeconds,
            result.Errors.Take(10).ToList()));
    }
}

/// <summary>
/// Request to generate questions for a subtopic.
/// </summary>
public record GenerateForSubtopicRequest(
    string CertificationId,
    string SubtopicId,
    string SubtopicName,
    int Count);

/// <summary>
/// Request to generate questions for a certification.
/// </summary>
public record GenerateForCertificationRequest(
    string CertificationId,
    int QuestionsPerSubtopic);

/// <summary>
/// Result of question generation.
/// </summary>
public record GenerationResultDto(
    int TotalGenerated,
    int TotalFailed,
    double DurationSeconds,
    List<string> Errors);
