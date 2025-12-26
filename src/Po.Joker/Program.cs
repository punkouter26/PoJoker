using Po.Joker.Components;
using Po.Joker.Features.Analysis;
using Po.Joker.Features.Diagnostics;
using Po.Joker.Features.Jokes;
using Po.Joker.Features.Leaderboards;
using Po.Joker.Shared.Contracts;
using Po.Joker.Infrastructure.Configuration;
using Po.Joker.Infrastructure.ExceptionHandling;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Infrastructure.Telemetry;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults (telemetry, health checks, resilience, service discovery)
builder.AddServiceDefaults();

// Configure Azure Key Vault for both local and deployed environments
builder.Configuration.AddPoJokerKeyVault(builder.Environment);

// Configure Serilog
builder.Host.UsePoJokerSerilog();

// Enable static web assets (required for Blazor WebAssembly files)
builder.WebHost.UseStaticWebAssets();

// Add services to the container
builder.Services.AddPoJokerBlazor();
builder.Services.AddPoJokerMediatR();
builder.Services.AddPoJokerTelemetry(builder.Configuration, builder.Environment);

// Add Azure Table Storage configuration
// Prefer explicit configuration using the storage account name (Managed Identity in Azure).
if (!string.IsNullOrEmpty(builder.Configuration["Azure:StorageAccountName"]))
{
    // Configure TableServiceClient using managed identity and the storage account name.
    builder.Services.AddPoJokerStorageLegacy(builder.Configuration, builder.Environment);
}
else
{
    // Fall back to Aspire-provided table client (e.g. local / AppHost scenarios)
    builder.AddAzureTableClient("tables");
}

builder.Services.AddPoJokerTableStorage();
builder.Services.AddScoped<IJokeStorageClient, JokeStorageClient>();

builder.Services.AddPoJokerHttpClients();
// Configure Azure OpenAI or use mock implementation to avoid external calls during local/E2E runs
if (!string.Equals(Environment.GetEnvironmentVariable("POJOKER_USE_MOCK_AI"), "true", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddPoJokerAzureOpenAI(builder.Configuration, builder.Environment);
}
else
{
    // Use MockAnalysisService for local E2E or CI environments to avoid hitting paid AI services
    builder.Services.AddScoped<IAnalysisService, MockAnalysisService>();
}

// Add exception handler
builder.Services.AddExceptionHandler<JesterExceptionHandler>();
builder.Services.AddProblemDetails();

// Add API documentation
builder.Services.AddPoJokerApiDocumentation();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<Po.Joker.Infrastructure.HealthChecks.JokeApiHealthCheck>("JokeAPI", tags: ["external"])
    .AddCheck<Po.Joker.Infrastructure.HealthChecks.AzureOpenAiHealthCheck>("AzureOpenAI", tags: ["external", "ai"])
    .AddCheck<Po.Joker.Infrastructure.HealthChecks.TableStorageHealthCheck>("TableStorage", tags: ["storage"]);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UsePoJokerSwagger(app.Environment);

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

// Add Serilog request logging
app.UsePoJokerRequestLogging();

app.UseAntiforgery();

// Map health check endpoint
app.MapHealthChecks("/health");

// Lightweight readiness endpoint for platform probes
app.MapGet("/healthz", () => Results.Ok("OK"));

// Map API endpoints
app.MapJokesEndpoints();
app.MapAnalysisEndpoints();
app.MapLeaderboardEndpoints();
app.MapDiagnosticsEndpoints();

app.MapStaticAssets();

// Configure Razor Components and interactive render modes. Avoid adding duplicate assemblies
var additionalAssemblies = new[]
{
    typeof(Po.Joker.Client._Imports).Assembly,
    typeof(Po.Joker.Components.App).Assembly
}
.Where(a => a != typeof(Program).Assembly)
.GroupBy(a => a.FullName)
.Select(g => g.First())
.ToArray();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(additionalAssemblies);

app.Run();
