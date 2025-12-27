using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithImage("mcr.microsoft.com/azure-storage/azurite");
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var tables = storage.AddTables("tables");

var api = builder.AddProject("api", "../Po.LearnCert.Api/Po.LearnCert.Api.csproj")
    .WithReference(tables)
    .WithExternalHttpEndpoints()
    .WaitFor(tables)
    .WithCommand("seed-db", "Seed Database", executeCommand: context =>
    {
        // Placeholder for triggering database seeding
        return Task.FromResult(new ExecuteCommandResult { Success = true });
    });

// The Client is hosted by the API, so we don't need to run it separately.
// However, if we wanted to run the Client standalone (e.g. for specialized testing), we could add it here.
// var web = builder.AddProject<Projects.Po_LearnCert_Client>("web")
//     .WithReference(api)
//     .WaitFor(api);

builder.Build().Run();