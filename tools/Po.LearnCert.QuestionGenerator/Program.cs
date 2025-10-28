using Azure.AI.OpenAI;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

// Parse command-line arguments
var questionsPerSubtopicArg = args.Length > 0 ? args[0] : null;
var skipConfirmation = args.Length > 1 && (args[1] == "-y" || args[1] == "--yes");

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Get configuration values
var tableConnectionString = configuration["AzureTableStorage:ConnectionString"];
var openAiEndpoint = configuration["AzureOpenAI:Endpoint"];
var openAiKey = configuration["AzureOpenAI:Key"];
var openAiDeployment = configuration["AzureOpenAI:DeploymentName"];
var defaultQuestionsPerSubtopic = configuration.GetValue<int>("SeedData:QuestionsPerSubtopic", 25);

Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║   PoLearnCert AI Question Generator               ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.WriteLine();

// Validate configuration
if (string.IsNullOrEmpty(tableConnectionString))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Error: Azure Table Storage connection string not configured");
    Console.ResetColor();
    return;
}

if (string.IsNullOrEmpty(openAiEndpoint) || string.IsNullOrEmpty(openAiKey) || string.IsNullOrEmpty(openAiDeployment))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Error: Azure OpenAI configuration incomplete");
    Console.WriteLine("   Please configure Endpoint, Key, and DeploymentName in appsettings.json");
    Console.ResetColor();
    return;
}

Console.WriteLine($"🔗 Storage: {(tableConnectionString.Contains("UseDevelopmentStorage") ? "Azurite (Local)" : "Azure")}");
Console.WriteLine($"🤖 AI Model: {openAiDeployment}");
Console.WriteLine();

// ============================================================================
// COMMAND-LINE USAGE
// ============================================================================

if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help"))
{
    Console.WriteLine("Usage: Po.LearnCert.QuestionGenerator [questionsPerSubtopic] [-y|--yes]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  questionsPerSubtopic  Number of questions to generate per subtopic (default: 25)");
    Console.WriteLine("  -y, --yes            Skip confirmation prompt");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  Po.LearnCert.QuestionGenerator              # Interactive mode");
    Console.WriteLine("  Po.LearnCert.QuestionGenerator 50           # Generate 50 questions per subtopic");
    Console.WriteLine("  Po.LearnCert.QuestionGenerator 100 -y       # Generate 100 questions without confirmation");
    return;
}

// ============================================================================
// PROMPT USER FOR NUMBER OF QUESTIONS
// ============================================================================

int questionsPerSubtopic;

// Check if provided via command-line
if (!string.IsNullOrWhiteSpace(questionsPerSubtopicArg) && int.TryParse(questionsPerSubtopicArg, out var cmdLineValue) && cmdLineValue > 0)
{
    questionsPerSubtopic = cmdLineValue;
    Console.WriteLine($"📊 Questions per subtopic: {questionsPerSubtopic} (from command-line)");
}
else
{
    // Interactive mode
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"📊 Enter number of questions to generate per subtopic (default: {defaultQuestionsPerSubtopic}): ");
        Console.ResetColor();

        var input = Console.ReadLine();

        // Use default if user presses Enter
        if (string.IsNullOrWhiteSpace(input))
        {
            questionsPerSubtopic = defaultQuestionsPerSubtopic;
            break;
        }

        // Validate input
        if (int.TryParse(input, out var parsedValue) && parsedValue > 0)
        {
            questionsPerSubtopic = parsedValue;
            break;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ Invalid input. Please enter a positive number.");
        Console.ResetColor();
    }
}

// ============================================================================
// SHOW SUMMARY AND CONFIRM
// ============================================================================

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("           GENERATION SUMMARY");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.ResetColor();

// Calculate totals
var subtopicCount = 8; // 4 AZ-900 + 4 Security+
var totalQuestions = questionsPerSubtopic * subtopicCount;
var estimatedMinutes = (int)Math.Ceiling(totalQuestions * 3.0 / 60); // ~3 seconds per question

Console.WriteLine($"  📚 Certifications: 2 (AZ-900, Security+)");
Console.WriteLine($"  📂 Subtopics: {subtopicCount}");
Console.WriteLine($"  ❓ Questions per subtopic: {questionsPerSubtopic}");
Console.WriteLine($"  🎯 Total questions: {totalQuestions}");
Console.WriteLine($"  ⏱️  Estimated time: ~{estimatedMinutes} minutes");
Console.WriteLine();

if (!skipConfirmation)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("⚠️  This will use Azure OpenAI API. Continue? (Y/n): ");
    Console.ResetColor();

    var confirmation = Console.ReadLine()?.Trim().ToLower();
    if (confirmation == "n" || confirmation == "no")
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("❌ Generation cancelled by user.");
        Console.ResetColor();
        return;
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✓ Confirmation skipped (--yes flag)");
    Console.ResetColor();
}

Console.WriteLine();

// Initialize clients
var tableServiceClient = new TableServiceClient(tableConnectionString);
var openAiClient = new AzureOpenAIClient(new Uri(openAiEndpoint!), new ApiKeyCredential(openAiKey!));
var chatClient = openAiClient.GetChatClient(openAiDeployment);

// Seed data
await SeedCertifications(tableServiceClient);
await SeedSubtopics(tableServiceClient);
await GenerateAndSeedQuestions(tableServiceClient, chatClient, questionsPerSubtopic);

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✓ Database seeding completed successfully!");
Console.ResetColor();

// ============================================================================
// SEED CERTIFICATIONS
// ============================================================================

async Task SeedCertifications(TableServiceClient serviceClient)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Seeding Certifications...");
    Console.ResetColor();

    var tableClient = serviceClient.GetTableClient("PoLearnCertCertifications");
    await tableClient.CreateIfNotExistsAsync();

    var certifications = new[]
    {
        new { RowKey = "AZ900", Name = "Microsoft Azure Fundamentals AZ-900", Description = "Demonstrate foundational knowledge of cloud concepts, core Azure services, security, privacy, compliance, trust, and Azure pricing and support." },
        new { RowKey = "SECPLUS", Name = "CompTIA Security+ SY0-701", Description = "Validate baseline cybersecurity skills and knowledge in risk management, cryptography, identity management, and security architecture." }
    };

    foreach (var cert in certifications)
    {
        var entity = new TableEntity("CERT", cert.RowKey)
        {
            { "Name", cert.Name },
            { "Description", cert.Description },
            { "TotalQuestions", 0 },
            { "CreatedDate", DateTime.UtcNow }
        };

        await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        Console.WriteLine($"  ✓ {cert.Name}");
    }
}

// ============================================================================
// SEED SUBTOPICS
// ============================================================================

async Task SeedSubtopics(TableServiceClient serviceClient)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\nSeeding Subtopics...");
    Console.ResetColor();

    var tableClient = serviceClient.GetTableClient("PoLearnCertSubtopics");
    await tableClient.CreateIfNotExistsAsync();

    var subtopics = new[]
    {
        // AZ-900 Subtopics
        new { PartitionKey = "AZ900", RowKey = "CLOUD_CONCEPTS", CertificationId = "AZ900", Name = "Cloud Concepts" },
        new { PartitionKey = "AZ900", RowKey = "CORE_SERVICES", CertificationId = "AZ900", Name = "Azure Core Services" },
        new { PartitionKey = "AZ900", RowKey = "SECURITY", CertificationId = "AZ900", Name = "Security, Privacy & Compliance" },
        new { PartitionKey = "AZ900", RowKey = "PRICING", CertificationId = "AZ900", Name = "Pricing & Support" },
        
        // Security+ Subtopics
        new { PartitionKey = "SECPLUS", RowKey = "THREATS", CertificationId = "SECPLUS", Name = "Threats, Attacks & Vulnerabilities" },
        new { PartitionKey = "SECPLUS", RowKey = "ARCHITECTURE", CertificationId = "SECPLUS", Name = "Architecture and Design" },
        new { PartitionKey = "SECPLUS", RowKey = "IMPLEMENTATION", CertificationId = "SECPLUS", Name = "Implementation" },
        new { PartitionKey = "SECPLUS", RowKey = "OPERATIONS", CertificationId = "SECPLUS", Name = "Operations and Incident Response" }
    };

    foreach (var subtopic in subtopics)
    {
        var entity = new TableEntity(subtopic.PartitionKey, subtopic.RowKey)
        {
            { "CertificationId", subtopic.CertificationId },
            { "Name", subtopic.Name },
            { "QuestionCount", 0 },
            { "CreatedDate", DateTime.UtcNow }
        };

        await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        Console.WriteLine($"  ✓ {subtopic.Name} ({subtopic.CertificationId})");
    }
}

// ============================================================================
// GENERATE AND SEED QUESTIONS USING AZURE OPENAI
// ============================================================================

async Task GenerateAndSeedQuestions(TableServiceClient serviceClient, ChatClient chatClient, int questionsPerSubtopic)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\nGenerating Questions using Azure OpenAI...");
    Console.ResetColor();

    var questionsTable = serviceClient.GetTableClient("PoLearnCertQuestions");
    await questionsTable.CreateIfNotExistsAsync();

    var choicesTable = serviceClient.GetTableClient("PoLearnCertAnswerChoices");
    await choicesTable.CreateIfNotExistsAsync();

    // Get all subtopics
    var subtopicsTable = serviceClient.GetTableClient("PoLearnCertSubtopics");
    var subtopics = subtopicsTable.QueryAsync<TableEntity>();

    int totalGenerated = 0;
    int totalFailed = 0;

    await foreach (var subtopic in subtopics)
    {
        var subtopicId = subtopic.RowKey;
        var certificationId = subtopic.GetString("CertificationId");
        var subtopicName = subtopic.GetString("Name");

        Console.WriteLine($"\n  Generating {questionsPerSubtopic} questions for: {subtopicName}");
        Console.Write("  ");

        for (int i = 1; i <= questionsPerSubtopic; i++)
        {
            var questionId = $"Q{subtopicId}-{i:D3}";

            // Generate question using Azure OpenAI
            var generatedQuestion = await GenerateQuestion(chatClient, subtopicName!, certificationId!);

            if (generatedQuestion != null)
            {
                // Create question entity
                var questionEntity = new TableEntity(certificationId, questionId)
                {
                    { "SubtopicId", subtopicId },
                    { "Text", generatedQuestion.Text },
                    { "Explanation", generatedQuestion.Explanation },
                    { "DifficultyLevel", generatedQuestion.DifficultyLevel },
                    { "CreatedDate", DateTime.UtcNow }
                };

                await questionsTable.UpsertEntityAsync(questionEntity, TableUpdateMode.Replace);

                // Create answer choice entities
                for (int choiceIndex = 0; choiceIndex < generatedQuestion.Choices.Count; choiceIndex++)
                {
                    var choice = generatedQuestion.Choices[choiceIndex];
                    var choiceId = $"C{questionId}-{choiceIndex + 1}";

                    var choiceEntity = new TableEntity(questionId, choiceId)
                    {
                        { "QuestionId", questionId },
                        { "Text", choice.Text },
                        { "IsCorrect", choice.IsCorrect },
                        { "Order", choiceIndex + 1 }
                    };

                    await choicesTable.UpsertEntityAsync(choiceEntity, TableUpdateMode.Replace);
                }

                totalGenerated++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("●");
                Console.ResetColor();
            }
            else
            {
                totalFailed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("×");
                Console.ResetColor();
            }
        }
        Console.WriteLine();
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  ✓ Generated {totalGenerated} questions successfully");
    if (totalFailed > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ Failed to generate {totalFailed} questions");
    }
    Console.ResetColor();
}

// ============================================================================
// GENERATE SINGLE QUESTION USING AZURE OPENAI
// ============================================================================

async Task<GeneratedQuestion?> GenerateQuestion(ChatClient chatClient, string subtopicName, string certificationId)
{
    const int maxRetries = 3;
    const int baseDelayMs = 1000; // Start with 1 second

    for (int attempt = 0; attempt <= maxRetries; attempt++)
    {
        try
        {
            var systemPrompt = @"You are an expert certification exam question writer. Generate realistic, high-quality multiple-choice questions for IT certification exams.

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
  ""text"": ""The question text here?"",
  ""explanation"": ""Detailed explanation of why the correct answer is right and others are wrong."",
  ""difficultyLevel"": ""Intermediate"",
  ""choices"": [
    {""text"": ""Choice A"", ""isCorrect"": false},
    {""text"": ""Choice B"", ""isCorrect"": true},
    {""text"": ""Choice C"", ""isCorrect"": false},
    {""text"": ""Choice D"", ""isCorrect"": false}
  ]
}";

            var userPrompt = $"Generate a certification exam question about '{subtopicName}' for the {certificationId} certification. Make it practical and scenario-based.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 500
            });

            var content = response.Value.Content[0].Text;

            // Clean up markdown formatting if present
            content = content.Trim();
            if (content.StartsWith("```json"))
            {
                content = content.Substring(7);
                if (content.EndsWith("```"))
                    content = content.Substring(0, content.Length - 3);
                content = content.Trim();
            }

            // Parse JSON response
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            var question = new GeneratedQuestion
            {
                Text = root.GetProperty("text").GetString()!,
                Explanation = root.GetProperty("explanation").GetString()!,
                DifficultyLevel = root.GetProperty("difficultyLevel").GetString()!,
                Choices = new List<GeneratedChoice>()
            };

            foreach (var choice in root.GetProperty("choices").EnumerateArray())
            {
                question.Choices.Add(new GeneratedChoice
                {
                    Text = choice.GetProperty("text").GetString()!,
                    IsCorrect = choice.GetProperty("isCorrect").GetBoolean()
                });
            }

            // Validate exactly 4 choices and exactly 1 correct answer
            if (question.Choices.Count != 4)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n    ⚠ Invalid question: Expected 4 choices, got {question.Choices.Count}");
                Console.ResetColor();
                return null;
            }

            var correctCount = question.Choices.Count(c => c.IsCorrect);
            if (correctCount != 1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n    ⚠ Invalid question: Expected 1 correct answer, got {correctCount}");
                Console.ResetColor();
                return null;
            }

            return question;
        }
        catch (ClientResultException ex) when (ex.Status == 429 && attempt < maxRetries)
        {
            // Rate limit hit - retry with exponential backoff
            var delayMs = baseDelayMs * (int)Math.Pow(2, attempt);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n    ⚠ Rate limit hit. Retrying in {delayMs}ms... (Attempt {attempt + 1}/{maxRetries})");
            Console.ResetColor();
            await Task.Delay(delayMs);
            continue;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            // Generic error - retry with shorter delay
            var delayMs = baseDelayMs;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n    ⚠ Error: {ex.Message}. Retrying in {delayMs}ms... (Attempt {attempt + 1}/{maxRetries})");
            Console.ResetColor();
            await Task.Delay(delayMs);
            continue;
        }
        catch (Exception ex)
        {
            // Final attempt failed
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n    ✗ Failed after {maxRetries} attempts: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    return null;
}

// ============================================================================
// HELPER CLASSES FOR GENERATED QUESTIONS
// ============================================================================

class GeneratedQuestion
{
    public string Text { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Intermediate";
    public List<GeneratedChoice> Choices { get; set; } = new();
}

class GeneratedChoice
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
