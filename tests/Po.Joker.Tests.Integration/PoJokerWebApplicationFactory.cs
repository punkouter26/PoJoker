using Azure.Data.Tables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Po.Joker.Features.Analysis;
using Po.Joker.Features.Jokes;
using Po.Joker.Contracts;
using Po.Joker.DTOs;

namespace Po.Joker.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures Azurite for storage and mocks AI + JokeAPI services.
/// </summary>
public class PoJokerWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Azurite connection string for local development storage.
    /// </summary>
    private const string AzuriteConnectionString = "UseDevelopmentStorage=true";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Disable Key Vault in tests — secrets are not needed for integration tests
        Environment.SetEnvironmentVariable("POJOKER_DISABLE_KEYVAULT", "true");
        // Use mock AI service to avoid requiring Azure OpenAI endpoint
        Environment.SetEnvironmentVariable("POJOKER_USE_MOCK_AI", "true");

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add connection string for Azurite and stub Azure config that would normally come from Key Vault
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:tables"] = AzuriteConnectionString,
                ["Azure:OpenAI:Endpoint"] = "https://test-openai.openai.azure.com/",
                ["Azure:OpenAI:DeploymentName"] = "gpt-4o-mini",
                ["Azure:KeyVaultUri"] = "https://test-kv.vault.azure.net/"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real AI service and replace with mock
            var analysisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAnalysisService));
            if (analysisDescriptor != null)
            {
                services.Remove(analysisDescriptor);
            }
            services.AddSingleton<IAnalysisService, MockAnalysisService>();

            // Replace IJokeApiClient with a deterministic mock — eliminates all external HTTP calls to JokeAPI
            services.RemoveAll<IJokeApiClient>();
            services.AddSingleton<IJokeApiClient, MockJokeApiClient>();

            // Replace TableServiceClient and TableClient with Azurite-backed instances for tests
            services.RemoveAll<TableServiceClient>();
            services.RemoveAll<TableClient>();

            // Add TableServiceClient pointing to Azurite
            services.AddSingleton<TableServiceClient>(_ => 
                new TableServiceClient(AzuriteConnectionString));

            // Add TableClient and ensure table exists synchronously to avoid race conditions
            services.AddSingleton<TableClient>(sp =>
            {
                var tableServiceClient = sp.GetRequiredService<TableServiceClient>();
                var tableClient = tableServiceClient.GetTableClient("jokeperformances");
                tableClient.CreateIfNotExists();
                return tableClient;
            });
        });
    }
}

/// <summary>
/// Mock implementation of IJokeApiClient — returns deterministic jokes without any external HTTP calls.
/// Implements the interface directly to guarantee the mock is always resolved by DI.
/// </summary>
internal sealed class MockJokeApiClient : IJokeApiClient
{
    private int _callCount;

    private static readonly JokeDto[] MockJokes =
    [
        new() { Id = 10, Category = "Programming", Type = "twopart", Setup = "Why do programmers prefer dark mode?",   Punchline = "Because light attracts bugs!",          SafeMode = true },
        new() { Id = 20, Category = "Pun",         Type = "twopart", Setup = "What do you call a fake noodle?",       Punchline = "An impasta!",                           SafeMode = true },
        new() { Id = 30, Category = "Misc",        Type = "twopart", Setup = "Why don't scientists trust atoms?",     Punchline = "Because they make up everything!",      SafeMode = true },
        new() { Id = 40, Category = "Dark",        Type = "twopart", Setup = "What is a skeleton's least fav room?",  Punchline = "The living room.",                      SafeMode = false },
    ];

    public Task<JokeDto> FetchJokeAsync(
        bool safeMode = true,
        IEnumerable<int>? excludeIds = null,
        CancellationToken cancellationToken = default)
    {
        var excluded = excludeIds?.ToHashSet() ?? [];
        var available = MockJokes.Where(j => !excluded.Contains(j.Id)).ToList();
        if (available.Count == 0)
            available = MockJokes.ToList(); // fallback: ignore exclusions if all excluded

        var index = Math.Abs(Interlocked.Increment(ref _callCount)) % available.Count;
        return Task.FromResult(available[index] with { SafeMode = safeMode });
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(["Programming", "Pun", "Misc", "Dark"]);
}
