using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Po.Joker.Infrastructure.Resilience;

/// <summary>
/// Polly resilience pipelines for external HTTP calls.
/// Implements retry with jitter, circuit breaker, and timeout per Constitution III.
/// </summary>
public static class PollyPipelines
{
    /// <summary>
    /// Configures resilience for the JokeAPI HTTP client.
    /// </summary>
    public static IHttpClientBuilder AddJokeApiResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("JokeApi", pipeline =>
        {
            // Retry with exponential backoff and jitter
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is HttpRequestException or TimeoutRejectedException ||
                    (args.Outcome.Result?.StatusCode is >= System.Net.HttpStatusCode.InternalServerError))
            });

            // Circuit breaker - open after 5 failures, stay open for 30 seconds
            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is HttpRequestException or TimeoutRejectedException ||
                    (args.Outcome.Result?.StatusCode is >= System.Net.HttpStatusCode.InternalServerError))
            });

            // Timeout - 10 seconds for JokeAPI
            pipeline.AddTimeout(TimeSpan.FromSeconds(10));
        });

        return builder;
    }

    /// <summary>
    /// Configures resilience for the Azure OpenAI HTTP client.
    /// Uses a longer timeout (15s) to account for AI processing time.
    /// </summary>
    public static IHttpClientBuilder AddAzureOpenAIResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("AzureOpenAI", pipeline =>
        {
            // Retry with exponential backoff and jitter
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromSeconds(2),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is HttpRequestException or TimeoutRejectedException ||
                    args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    (args.Outcome.Result?.StatusCode is >= System.Net.HttpStatusCode.InternalServerError))
            });

            // Circuit breaker - open after 5 failures
            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(60),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is HttpRequestException or TimeoutRejectedException ||
                    (args.Outcome.Result?.StatusCode is >= System.Net.HttpStatusCode.InternalServerError))
            });

            // Timeout - 15 seconds for AI processing (per spec SC-003)
            pipeline.AddTimeout(TimeSpan.FromSeconds(15));
        });

        return builder;
    }

    /// <summary>
    /// Configures resilience for Azure Table Storage operations.
    /// </summary>
    public static IHttpClientBuilder AddTableStorageResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("TableStorage", pipeline =>
        {
            // Retry with exponential backoff
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is HttpRequestException or TimeoutRejectedException ||
                    args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            });

            // Timeout - 5 seconds for storage operations
            pipeline.AddTimeout(TimeSpan.FromSeconds(5));
        });

        return builder;
    }
}
