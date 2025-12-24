var builder = DistributedApplication.CreateBuilder(args);

// Azure Table Storage - uses Azurite emulator in development
// WithLifetime(Persistent) prevents container restart between debug sessions
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

var tables = storage.AddTables("tables");

// Main Blazor application with Table Storage reference
var jokerApp = builder.AddProject<Projects.Po_Joker>("joker")
    .WithReference(tables)
    .WaitFor(tables)
    .WithExternalHttpEndpoints();

builder.Build().Run();
