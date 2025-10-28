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
    private readonly AzuriteFixture _azuriteFixture;

    public TestWebApplicationFactory(AzuriteFixture azuriteFixture)
    {
        _azuriteFixture = azuriteFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override connection string to use Azurite
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AzureTableStorage"] = _azuriteFixture.ConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Additional service overrides for testing can be added here
        });

        builder.UseEnvironment("Testing");
    }
}
