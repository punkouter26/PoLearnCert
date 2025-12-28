using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        // Use short image name so Aspire prefixes the registry and avoids accidental double-prefixing
        azurite.WithImage("azure-storage/azurite:latest");
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var tables = storage.AddTables("tables");

var api = builder.AddProject<Projects.Po_LearnCert_Api>("api")
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