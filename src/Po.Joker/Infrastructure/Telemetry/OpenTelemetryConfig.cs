using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Po.Joker.Infrastructure.Telemetry;

/// <summary>
/// OpenTelemetry configuration for distributed tracing and metrics.
/// Uses standard AspNetCore and HttpClient instrumentation only.
/// </summary>
public static class OpenTelemetryConfig
{
    private const string ServiceName = "Po.Joker";
    private const string ServiceVersion = "1.0.0";

    /// <summary>
    /// Configures OpenTelemetry for the application.
    /// </summary>
    public static IServiceCollection AddPoJokerTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (!environment.IsDevelopment() && string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            throw new InvalidOperationException("Application Insights connection string is required outside Development.");
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }

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
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }

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


