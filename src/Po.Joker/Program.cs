using Po.Joker.Components;
using Po.Joker.Features.Analysis;
using Po.Joker.Features.Diagnostics;
using Po.Joker.Features.Jokes;
using Po.Joker.Features.Leaderboards;
using Po.Joker.Infrastructure.Configuration;
using Po.Joker.Infrastructure.ExceptionHandling;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Infrastructure.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults (telemetry, health checks, resilience, service discovery)
builder.AddServiceDefaults();

// Configure Azure Key Vault for both local and deployed environments
builder.Configuration.AddPoJokerKeyVault(builder.Environment);

// Configure Serilog
builder.Host.UsePoJokerSerilog();

// Add services to the container
builder.Services.AddPoJokerBlazor();
builder.Services.AddPoJokerMediatR();
builder.Services.AddPoJokerTelemetry(builder.Configuration);

// Add Aspire Azure Table Storage (connection managed by Aspire AppHost)
builder.AddAzureTableClient("tables");
builder.Services.AddPoJokerTableStorage();
builder.Services.AddScoped<IJokeStorageClient, JokeStorageClient>();

builder.Services.AddPoJokerHttpClients();
builder.Services.AddPoJokerAzureOpenAI(builder.Configuration, builder.Environment);

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

// Add Serilog request logging
app.UsePoJokerRequestLogging();

app.UseAntiforgery();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map API endpoints
app.MapJokesEndpoints();
app.MapAnalysisEndpoints();
app.MapLeaderboardEndpoints();
app.MapDiagnosticsEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Po.Joker.Client._Imports).Assembly);

app.Run();
