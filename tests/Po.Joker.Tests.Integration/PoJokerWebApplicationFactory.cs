using Azure.Data.Tables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Po.Joker.Features.Analysis;
using Po.Joker.Features.Jokes;
using Po.Joker.Shared.Contracts;
using Po.Joker.Shared.DTOs;

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

            // Remove real JokeApiClient and replace with mock to eliminate external HTTP calls
            services.RemoveAll<IJokeApiClient>();
            services.AddSingleton<IJokeApiClient, MockJokeApiClient>();

            // Replace TableServiceClient with Azurite-backed instance for tests
            var tableServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TableServiceClient));
            if (tableServiceDescriptor != null)
            {
                services.Remove(tableServiceDescriptor);
            }

            // Add TableServiceClient pointing to Azurite
            services.AddSingleton<TableServiceClient>(_ => 
                new TableServiceClient(AzuriteConnectionString));
        });
    }
}

/// <summary>
/// Mock JokeApiClient that returns deterministic jokes without external HTTP calls.
/// Eliminates network dependency on v2.jokeapi.dev and improves test speed/reliability.
/// </summary>
internal sealed class MockJokeApiClient : IJokeApiClient
{
    private int _callCount;

    private static readonly JokeDto[] MockJokes =
    [
        new()
        {
            Id = 1,
            Category = "Programming",
            Type = "twopart",
            Setup = "Why do programmers prefer dark mode?",
            Punchline = "Because light attracts bugs!",
            SafeMode = true
        },
        new()
        {
            Id = 2,
            Category = "Pun",
            Type = "twopart",
            Setup = "What do you call a fake noodle?",
            Punchline = "An impasta!",
            SafeMode = true
        },
        new()
        {
            Id = 3,
            Category = "Misc",
            Type = "twopart",
            Setup = "Why don't scientists trust atoms?",
            Punchline = "Because they make up everything!",
            SafeMode = true
        }
    ];

    public Task<JokeDto> FetchJokeAsync(
        bool safeMode = true,
        IEnumerable<int>? excludeIds = null,
        CancellationToken cancellationToken = default)
    {
        var index = Interlocked.Increment(ref _callCount) % MockJokes.Length;
        var joke = MockJokes[index] with { SafeMode = safeMode };
        return Task.FromResult(joke);
    }
}
