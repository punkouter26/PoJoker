using Po.Joker.Components;
using Po.Joker.Features.Analysis;
using Po.Joker.Features.Diagnostics;
using Po.Joker.Features.Jokes;
using Po.Joker.Features.Leaderboards;
using Po.Joker.Contracts;
using Po.Joker.Infrastructure.Configuration;
using Po.Joker.Infrastructure.ExceptionHandling;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Infrastructure.Telemetry;

// Tee Console.Out → logs/console.log so startup messages are captured alongside Serilog output
Directory.CreateDirectory("logs");
var consoleLogFile = new StreamWriter("logs/console.log", append: false) { AutoFlush = true };
Console.SetOut(new ConsoleTee(Console.Out, consoleLogFile));
Console.SetError(new ConsoleTee(Console.Error, consoleLogFile));

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault for both local and deployed environments
builder.Configuration.AddPoJokerKeyVault(builder.Environment);

// Configure Serilog
builder.Host.UsePoJokerSerilog();

// Enable static web assets (only needed in Development; published apps already include them)
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

// Add services to the container
builder.Services.AddPoJokerBlazor(builder.Configuration);
builder.Services.AddPoJokerMediatR();
builder.Services.AddPoJokerTelemetry(builder.Configuration, builder.Environment);

// Add Azure Table Storage (uses Azurite locally, Managed Identity in production)
builder.Services.AddPoJokerStorage(builder.Configuration, builder.Environment);
builder.Services.AddScoped<IJokeStorageClient, JokeStorageClient>();

builder.Services.AddPoJokerHttpClients();
// Use mock AI if explicitly requested OR if the OpenAI endpoint is missing (Key Vault not reachable)
var useMockAi = string.Equals(Environment.GetEnvironmentVariable("POJOKER_USE_MOCK_AI"), "true", StringComparison.OrdinalIgnoreCase)
    || string.Equals(builder.Configuration["POJOKER_USE_MOCK_AI"], "true", StringComparison.OrdinalIgnoreCase)
    || string.IsNullOrEmpty(builder.Configuration["Azure:OpenAI:Endpoint"]);
if (!useMockAi)
{
    builder.Services.AddPoJokerAzureOpenAI(builder.Configuration, builder.Environment);
}
else
{
    if (string.IsNullOrEmpty(builder.Configuration["Azure:OpenAI:Endpoint"]))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[PoJoker] Azure:OpenAI:Endpoint not configured — using MockAnalysisService.");
        Console.ResetColor();
    }
    builder.Services.AddScoped<IAnalysisService, MockAnalysisService>();
}

// Add exception handler
builder.Services.AddExceptionHandler<JesterExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<Po.Joker.Infrastructure.HealthChecks.JokeApiHealthCheck>("JokeAPI", tags: ["external"])
    .AddCheck<Po.Joker.Infrastructure.HealthChecks.AzureOpenAiHealthCheck>("AzureOpenAI", tags: ["external", "ai"])
    .AddCheck<Po.Joker.Infrastructure.HealthChecks.TableStorageHealthCheck>("TableStorage", tags: ["storage"]);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
consoleLogFile.Dispose();

/// <summary>Writes to both an original TextWriter and a log file simultaneously.</summary>
sealed class ConsoleTee(TextWriter primary, TextWriter file) : TextWriter
{
    public override System.Text.Encoding Encoding => primary.Encoding;
    public override void Write(char value) { primary.Write(value); file.Write(value); }
    public override void Write(string? value) { primary.Write(value); file.Write(value); }
    public override void WriteLine(string? value) { primary.WriteLine(value); file.WriteLine(value); }
    public override void WriteLine() { primary.WriteLine(); file.WriteLine(); }
    protected override void Dispose(bool disposing) { if (disposing) file.Dispose(); base.Dispose(disposing); }
}
