using System.Diagnostics;
using System.Net.Sockets;

namespace Po.LearnCert.IntegrationTests.Infrastructure;

/// <summary>
/// Test fixture for starting and stopping Azurite storage emulator.
/// </summary>
public class AzuriteFixture : IDisposable
{
    private Process? _azuriteProcess;
    private readonly string _connectionString = "UseDevelopmentStorage=true";
    private readonly string _dataPath;

    public string ConnectionString => _connectionString;

    public AzuriteFixture()
    {
        _dataPath = Path.Combine(Path.GetTempPath(), "PoLearnCert", "azurite-data", Guid.NewGuid().ToString("n"));
        StartAzurite();

        // Wait for Azurite to be ready (table endpoint)
        WaitForPort(host: "127.0.0.1", port: 10002, timeout: TimeSpan.FromSeconds(60));
    }

    private void StartAzurite()
    {
        var attempts = new (string command, string arguments)[]
        {
            ("azurite", $"--silent --location \"{_dataPath}\""),
            ("npx", $"--yes azurite --silent --location \"{_dataPath}\""),
            ("npx", $"-y azurite --silent --location \"{_dataPath}\""),
        };

        foreach (var (command, arguments) in attempts)
        {
            try
            {
                var startInfo = CreateStartInfo(command, arguments);
                _azuriteProcess = new Process
                {
                    StartInfo = startInfo
                };

                _azuriteProcess.Start();

                // Give it a moment; if it exits immediately, try the next launch strategy.
                if (_azuriteProcess.WaitForExit(200))
                {
                    var stderr = _azuriteProcess.StandardError.ReadToEnd();
                    var stdout = _azuriteProcess.StandardOutput.ReadToEnd();
                    _azuriteProcess.Dispose();
                    _azuriteProcess = null;

                    if (string.IsNullOrWhiteSpace(stderr) && string.IsNullOrWhiteSpace(stdout))
                    {
                        continue;
                    }

                    continue;
                }

                return;
            }
            catch
            {
                _azuriteProcess?.Dispose();
                _azuriteProcess = null;
            }
        }

        throw new InvalidOperationException(
            "Azurite executable not found. Install it with 'npm install -g azurite', or ensure Node/npm is available so 'npx --yes azurite' can run.");
    }

    private static ProcessStartInfo CreateStartInfo(string command, string arguments)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command} {arguments}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
        }

        return new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
    }

    private static void WaitForPort(string host, int port, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                if (connectTask.Wait(TimeSpan.FromMilliseconds(250)) && client.Connected)
                {
                    return;
                }
            }
            catch
            {
                // ignored
            }

            Thread.Sleep(250);
        }

        throw new TimeoutException($"Azurite did not open {host}:{port} within {timeout.TotalSeconds:0}s.");
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
        if (Directory.Exists(_dataPath))
        {
            try
            {
                Directory.Delete(_dataPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
