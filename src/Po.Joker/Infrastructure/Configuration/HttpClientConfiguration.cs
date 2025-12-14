using Po.Joker.Features.Jokes;
using Po.Joker.Infrastructure.Resilience;
using Po.Joker.Shared.Contracts;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring HTTP clients with resilience.
/// </summary>
public static class HttpClientConfiguration
{
    /// <summary>
    /// Adds JokeAPI HTTP client with Polly resilience policies.
    /// </summary>
    public static IServiceCollection AddPoJokerHttpClients(this IServiceCollection services)
    {
        // Add JokeAPI client with resilience
        services.AddHttpClient<IJokeApiClient, JokeApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://v2.jokeapi.dev/");
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddJokeApiResilience();

        // Add named HttpClient for JokeAPI health check
        services.AddHttpClient("JokeApi", client =>
        {
            client.BaseAddress = new Uri("https://v2.jokeapi.dev/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
