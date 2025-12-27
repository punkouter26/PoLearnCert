using Azure.Data.Tables;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;

namespace Po.LearnCert.Api.Infrastructure;

/// <summary>
/// Seeds initial data into Azure Table Storage.
/// </summary>
public class DataSeeder
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(TableServiceClient tableServiceClient, ILogger<DataSeeder> logger)
    {
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedCertificationsAsync();
        await SeedQuestionsAsync();
    }

    private async Task SeedCertificationsAsync()
    {
        var table = _tableServiceClient.GetTableClient("PoLearnCertCertifications");
        await table.CreateIfNotExistsAsync();

        // Check if already seeded
        var count = 0;
        await foreach (var _ in table.QueryAsync<CertificationEntity>(maxPerPage: 1))
        {
            count++;
            break;
        }

        if (count > 0)
        {
            _logger.LogInformation("Certifications already seeded");
            return;
        }

        _logger.LogInformation("Seeding certifications...");

        var certifications = new[]
        {
            new CertificationEntity
            {
                PartitionKey = "CERT",
                RowKey = "az-900",
                Id = "az-900",
                Name = "Microsoft Azure Fundamentals AZ-900",
                Description = "Azure Fundamentals",
                Icon = "☁️",
                TotalQuestions = 200,
                PassingScore = 70
            },
            new CertificationEntity
            {
                PartitionKey = "CERT",
                RowKey = "sy0-701",
                Id = "sy0-701",
                Name = "CompTIA Security+ SY0-701",
                Description = "Security Fundamentals",
                Icon = "🔒",
                TotalQuestions = 200,
                PassingScore = 75
            }
        };

        foreach (var cert in certifications)
        {
            await table.AddEntityAsync(cert);
            _logger.LogInformation("Seeded certification: {Name}", cert.Name);
        }

        _logger.LogInformation("Certifications seeded successfully");
    }

    private async Task SeedQuestionsAsync()
    {
        var questionsTable = _tableServiceClient.GetTableClient("PoLearnCertQuestions");
        await questionsTable.CreateIfNotExistsAsync();

        var choicesTable = _tableServiceClient.GetTableClient("PoLearnCertAnswerChoices");
        await choicesTable.CreateIfNotExistsAsync();

        // Check if already seeded
        var count = 0;
        await foreach (var _ in questionsTable.QueryAsync<QuestionEntity>(maxPerPage: 1))
        {
            count++;
            break;
        }

        if (count > 0)
        {
            _logger.LogInformation("Questions already seeded");
            return;
        }

        _logger.LogInformation("Seeding questions...");

        var questions = new List<(QuestionEntity Question, List<AnswerChoiceEntity> Choices)>();

        // AZ-900 Questions
        for (int i = 1; i <= 5; i++)
        {
            var qId = Guid.NewGuid().ToString();
            var question = new QuestionEntity
            {
                PartitionKey = "az-900", // CertificationId
                RowKey = qId,
                QuestionId = qId,
                CertificationId = "az-900",
                SubtopicId = "cloud-concepts",
                Text = $"Sample AZ-900 Question {i}: What is Cloud Computing?",
                Explanation = "Cloud computing is the delivery of computing services over the Internet.",
                DifficultyLevel = "Easy",
                CreatedDate = DateTime.UtcNow
            };

            var choices = new List<AnswerChoiceEntity>
            {
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Delivery of services over Internet", IsCorrect = true, Order = 1 },
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Computing on local mainframe", IsCorrect = false, Order = 2 },
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Storing data on floppy disks", IsCorrect = false, Order = 3 },
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "None of the above", IsCorrect = false, Order = 4 }
            };
            questions.Add((question, choices));
        }

        // SY0-701 Questions
        for (int i = 1; i <= 5; i++)
        {
            var qId = Guid.NewGuid().ToString();
            var question = new QuestionEntity
            {
                PartitionKey = "sy0-701",
                RowKey = qId,
                QuestionId = qId,
                CertificationId = "sy0-701",
                SubtopicId = "threats",
                Text = $"Sample Security+ Question {i}: What is Phishing?",
                Explanation = "Phishing is a cybercrime in which a target is contacted by email...",
                DifficultyLevel = "Easy",
                CreatedDate = DateTime.UtcNow
            };

            var choices = new List<AnswerChoiceEntity>
            {
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Social engineering attack", IsCorrect = true, Order = 1 },
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Fishing sport", IsCorrect = false, Order = 2 },
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Network protocol", IsCorrect = false, Order = 3 },
                new AnswerChoiceEntity { PartitionKey = qId, RowKey = Guid.NewGuid().ToString(), ChoiceId = Guid.NewGuid().ToString(), QuestionId = qId, Text = "Encryption algorithm", IsCorrect = false, Order = 4 }
            };
            questions.Add((question, choices));
        }

        foreach (var (q, cList) in questions)
        {
            await questionsTable.AddEntityAsync(q);
            foreach (var c in cList)
            {
                await choicesTable.AddEntityAsync(c);
            }
        }

        _logger.LogInformation("Questions seeded successfully");
    }
}
