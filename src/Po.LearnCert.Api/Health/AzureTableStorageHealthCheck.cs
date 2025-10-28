using Microsoft.Extensions.Diagnostics.HealthChecks;
using Azure.Data.Tables;

namespace Po.LearnCert.Api.Health;

public class AzureTableStorageHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<AzureTableStorageHealthCheck> _logger;

    public AzureTableStorageHealthCheck(
        IConfiguration configuration,
        ILogger<AzureTableStorageHealthCheck> logger)
    {
        _connectionString = configuration["AzureTableStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Table Storage connection string not configured");
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceClient = new TableServiceClient(_connectionString);
            
            // Attempt to get service properties to verify connection
            await serviceClient.GetPropertiesAsync(cancellationToken);

            return HealthCheckResult.Healthy("Azure Table Storage is accessible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Table Storage health check failed");
            return HealthCheckResult.Unhealthy(
                "Azure Table Storage is not accessible",
                ex);
        }
    }
}
