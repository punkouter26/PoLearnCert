using System.Diagnostics;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Data.Tables;
using OpenAI.Chat;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Po.LearnCert.Api.Features.QuestionGeneration;

/// <summary>
/// Service for generating certification exam questions using Azure OpenAI.
/// </summary>
public class QuestionGenerationService : IQuestionGenerationService
{
    private readonly TableClient _questionsTable;
    private readonly TableClient _choicesTable;
    private readonly ISubtopicRepository _subtopicRepository;
    private readonly ChatClient _chatClient;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<QuestionGenerationService> _logger;

    public QuestionGenerationService(
        TableServiceClient tableServiceClient,
        ISubtopicRepository subtopicRepository,
        AzureOpenAIClient openAiClient,
        IConfiguration configuration,
        ILogger<QuestionGenerationService> logger)
    {
        _subtopicRepository = subtopicRepository;
        _logger = logger;

        _questionsTable = tableServiceClient.GetTableClient("PoLearnCertQuestions");
        _choicesTable = tableServiceClient.GetTableClient("PoLearnCertAnswerChoices");

        _questionsTable.CreateIfNotExists();
        _choicesTable.CreateIfNotExists();

        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";
        _chatClient = openAiClient.GetChatClient(deploymentName);

        // Polly resilience pipeline for API calls
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(1)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
    }

    public async Task<QuestionGenerationResult> GenerateQuestionsAsync(
        string certificationId,
        string subtopicId,
        string subtopicName,
        int count,
        IProgress<QuestionGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();
        int generated = 0;
        int failed = 0;

        _logger.LogInformation("Starting generation of {Count} questions for {Subtopic}",
            count, subtopicName);

        for (int i = 1; i <= count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var questionId = $"Q{subtopicId}-{Guid.NewGuid():N}"[..20];

            try
            {
                var question = await GenerateSingleQuestionAsync(subtopicName, certificationId, cancellationToken);

                if (question != null)
                {
                    await SaveQuestionAsync(certificationId, subtopicId, questionId, question, cancellationToken);
                    generated++;
                    _logger.LogDebug("Generated question {Index}/{Total} for {Subtopic}",
                        i, count, subtopicName);
                }
                else
                {
                    failed++;
                    errors.Add($"Question {i}: Failed to generate valid question");
                }
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Question {i}: {ex.Message}");
                _logger.LogWarning(ex, "Failed to generate question {Index} for {Subtopic}",
                    i, subtopicName);
            }

            progress?.Report(new QuestionGenerationProgress(
                subtopicName,
                i,
                count,
                generated,
                failed,
                errors.LastOrDefault()));
        }

        stopwatch.Stop();
        _logger.LogInformation("Completed generation for {Subtopic}: {Generated} generated, {Failed} failed in {Duration}",
            subtopicName, generated, failed, stopwatch.Elapsed);

        return new QuestionGenerationResult(generated, failed, stopwatch.Elapsed, errors);
    }

    public async Task<QuestionGenerationResult> GenerateQuestionsForCertificationAsync(
        string certificationId,
        int questionsPerSubtopic,
        IProgress<QuestionGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var allErrors = new List<string>();
        int totalGenerated = 0;
        int totalFailed = 0;

        var subtopics = await _subtopicRepository.GetByCertificationIdAsync(certificationId);

        foreach (var subtopic in subtopics)
        {
            var result = await GenerateQuestionsAsync(
                certificationId,
                subtopic.SubtopicId,
                subtopic.Name,
                questionsPerSubtopic,
                progress,
                cancellationToken);

            totalGenerated += result.TotalGenerated;
            totalFailed += result.TotalFailed;
            allErrors.AddRange(result.Errors);
        }

        stopwatch.Stop();
        return new QuestionGenerationResult(totalGenerated, totalFailed, stopwatch.Elapsed, allErrors);
    }

    private async Task<GeneratedQuestion?> GenerateSingleQuestionAsync(
        string subtopicName,
        string certificationId,
        CancellationToken cancellationToken)
    {
        return await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            var systemPrompt = """
                You are an expert certification exam question writer. Generate realistic, high-quality multiple-choice questions for IT certification exams.

                Requirements:
                1. Generate ONE question with EXACTLY 4 answer choices
                2. Only ONE choice should be correct
                3. Include a detailed explanation (2-3 sentences) explaining why the correct answer is right
                4. Set appropriate difficulty level: Beginner, Intermediate, or Advanced
                5. Make distractors (wrong answers) plausible but clearly incorrect to someone with proper knowledge
                6. Focus on practical scenarios and real-world applications
                7. Return ONLY valid JSON, no additional text or markdown formatting

                JSON Format:
                {
                  "text": "The question text here?",
                  "explanation": "Detailed explanation of why the correct answer is right and others are wrong.",
                  "difficultyLevel": "Intermediate",
                  "choices": [
                    {"text": "Choice A", "isCorrect": false},
                    {"text": "Choice B", "isCorrect": true},
                    {"text": "Choice C", "isCorrect": false},
                    {"text": "Choice D", "isCorrect": false}
                  ]
                }
                """;

            var userPrompt = $"Generate a certification exam question about '{subtopicName}' for the {certificationId} certification. Make it practical and scenario-based.";

            List<ChatMessage> messages =
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ];

            var response = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 500
            }, ct);

            var content = response.Value.Content[0].Text.Trim();

            // Clean up markdown formatting if present
            if (content.StartsWith("```json"))
            {
                content = content[7..];
                if (content.EndsWith("```"))
                    content = content[..^3];
                content = content.Trim();
            }

            // Parse JSON response
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            var choices = root.GetProperty("choices").EnumerateArray()
                .Select(choice => new GeneratedChoice(
                    choice.GetProperty("text").GetString()!,
                    choice.GetProperty("isCorrect").GetBoolean()))
                .ToList();

            var question = new GeneratedQuestion(
                root.GetProperty("text").GetString()!,
                root.GetProperty("explanation").GetString()!,
                root.GetProperty("difficultyLevel").GetString()!,
                choices);

            // Validate
            if (question.Choices.Count != 4)
            {
                _logger.LogWarning("Invalid question: Expected 4 choices, got {Count}", question.Choices.Count);
                return null;
            }

            var correctCount = question.Choices.Count(c => c.IsCorrect);
            if (correctCount != 1)
            {
                _logger.LogWarning("Invalid question: Expected 1 correct answer, got {Count}", correctCount);
                return null;
            }

            return question;
        }, cancellationToken);
    }

    private async Task SaveQuestionAsync(
        string certificationId,
        string subtopicId,
        string questionId,
        GeneratedQuestion question,
        CancellationToken cancellationToken)
    {
        // Save question
        var questionEntity = new TableEntity(certificationId, questionId)
        {
            { "QuestionId", questionId },
            { "SubtopicId", subtopicId },
            { "CertificationId", certificationId },
            { "Text", question.Text },
            { "Explanation", question.Explanation },
            { "DifficultyLevel", question.DifficultyLevel },
            { "CreatedDate", DateTime.UtcNow }
        };

        await _questionsTable.UpsertEntityAsync(questionEntity, TableUpdateMode.Replace, cancellationToken);

        // Save choices
        for (int i = 0; i < question.Choices.Count; i++)
        {
            var choice = question.Choices[i];
            var choiceId = $"C{questionId}-{i + 1}";

            var choiceEntity = new TableEntity(questionId, choiceId)
            {
                { "ChoiceId", choiceId },
                { "QuestionId", questionId },
                { "Text", choice.Text },
                { "IsCorrect", choice.IsCorrect },
                { "Order", i + 1 }
            };

            await _choicesTable.UpsertEntityAsync(choiceEntity, TableUpdateMode.Replace, cancellationToken);
        }
    }

    private record GeneratedQuestion(
        string Text,
        string Explanation,
        string DifficultyLevel,
        List<GeneratedChoice> Choices);

    private record GeneratedChoice(
        string Text,
        bool IsCorrect);
}
