using Azure.Data.Tables;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;

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
}
