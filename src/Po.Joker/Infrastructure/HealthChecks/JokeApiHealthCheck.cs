using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Po.Joker.Infrastructure.HealthChecks;

/// <summary>
/// Health check for JokeAPI external dependency.
/// </summary>
public sealed class JokeApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<JokeApiHealthCheck> _logger;

    public JokeApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<JokeApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("JokeApi");
            var response = await client.GetAsync("/info", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("JokeAPI is responding normally.");
            }

            _logger.LogWarning("JokeAPI health check returned {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded($"JokeAPI returned status code: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "JokeAPI health check failed");
            return HealthCheckResult.Unhealthy("Unable to reach JokeAPI.", ex);
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded("JokeAPI request timed out.");
        }
    }
}
