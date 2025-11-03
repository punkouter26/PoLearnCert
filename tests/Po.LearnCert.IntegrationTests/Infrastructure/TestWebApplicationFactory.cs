using Azure.Data.Tables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Po.LearnCert.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing with Azurite.
/// </summary>
/// <typeparam name="TProgram">The entry point of the API</typeparam>
public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing TableServiceClient registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TableServiceClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register a new TableServiceClient with development storage connection string
            services.AddSingleton(new TableServiceClient("UseDevelopmentStorage=true"));
        });

        builder.UseEnvironment("Testing");
    }
}

/// <summary>
/// Custom WebApplicationFactory for integration testing with explicit Azurite fixture.
/// </summary>
/// <typeparam name="TProgram">The entry point of the API</typeparam>
public class TestWebApplicationFactoryWithAzurite<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly AzuriteFixture _azuriteFixture;

    public TestWebApplicationFactoryWithAzurite(AzuriteFixture azuriteFixture)
    {
        _azuriteFixture = azuriteFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing TableServiceClient registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TableServiceClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register a new TableServiceClient with Azurite connection string
            services.AddSingleton(new TableServiceClient(_azuriteFixture.ConnectionString));
        });

        builder.UseEnvironment("Testing");
    }
}
