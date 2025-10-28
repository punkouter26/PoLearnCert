using System.Diagnostics;

namespace Po.LearnCert.IntegrationTests.Infrastructure;

/// <summary>
/// Test fixture for starting and stopping Azurite storage emulator.
/// </summary>
public class AzuriteFixture : IDisposable
{
    private Process? _azuriteProcess;
    private readonly string _connectionString = "UseDevelopmentStorage=true";

    public string ConnectionString => _connectionString;

    public AzuriteFixture()
    {
        StartAzurite();

        // Wait for Azurite to be ready
        Thread.Sleep(3000);
    }

    private void StartAzurite()
    {
        try
        {
            _azuriteProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "azurite",
                    Arguments = "--silent --location azurite-data",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _azuriteProcess.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to start Azurite. Make sure Azurite is installed (npm install -g azurite)", ex);
        }
    }

    public void Dispose()
    {
        if (_azuriteProcess != null && !_azuriteProcess.HasExited)
        {
            _azuriteProcess.Kill(entireProcessTree: true);
            _azuriteProcess.WaitForExit();
            _azuriteProcess.Dispose();
        }

        // Clean up Azurite data
        var azuriteDataPath = Path.Combine(Directory.GetCurrentDirectory(), "azurite-data");
        if (Directory.Exists(azuriteDataPath))
        {
            try
            {
                Directory.Delete(azuriteDataPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
