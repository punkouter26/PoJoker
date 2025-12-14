using System.Diagnostics;
using System.Diagnostics.Metrics;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Po.Joker.Infrastructure.Telemetry;

/// <summary>
/// OpenTelemetry configuration for distributed tracing and metrics.
/// </summary>
public static class OpenTelemetryConfig
{
    public const string ServiceName = "Po.Joker";
    public const string ServiceVersion = "1.0.0";

    /// <summary>
    /// ActivitySource for custom tracing spans.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// Meter for custom metrics.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);

    // Custom metrics
    public static readonly Counter<long> JokesFetched = Meter.CreateCounter<long>(
        "jokes.fetched",
        description: "Number of jokes fetched from external API");

    public static readonly Counter<long> JokesAnalyzed = Meter.CreateCounter<long>(
        "jokes.analyzed",
        description: "Number of jokes analyzed by AI");

    public static readonly Counter<long> AiTriumphs = Meter.CreateCounter<long>(
        "ai.triumphs",
        description: "Number of times AI correctly guessed the punchline");

    public static readonly Histogram<double> AiLatency = Meter.CreateHistogram<double>(
        "ai.latency.ms",
        unit: "ms",
        description: "AI punchline prediction latency in milliseconds");

    public static readonly Histogram<double> JokeApiLatency = Meter.CreateHistogram<double>(
        "jokeapi.latency.ms",
        unit: "ms",
        description: "JokeAPI request latency in milliseconds");

    /// <summary>
    /// Configures OpenTelemetry for the application.
    /// </summary>
    public static IServiceCollection AddPoJokerTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ServiceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();

                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    tracing.AddAzureMonitorTraceExporter(options =>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(ServiceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();

                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    metrics.AddAzureMonitorMetricExporter(options =>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                    });
                }
            });

        return services;
    }
}
