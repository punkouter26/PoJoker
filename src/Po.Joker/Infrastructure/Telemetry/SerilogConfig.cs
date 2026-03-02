using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Po.Joker.Infrastructure.Telemetry;

/// <summary>
/// Serilog structured logging configuration.
/// </summary>
public static class SerilogConfig
{
    /// <summary>
    /// Configures Serilog for the application.
    /// </summary>
    public static IHostBuilder UsePoJokerSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            var appInsightsConnectionString = context.Configuration["ApplicationInsights:ConnectionString"]
                ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Po.Joker")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.File(
                    path: "logs/pojoker-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    shared: true);

            if (context.HostingEnvironment.IsDevelopment())
            {
                // Development: Console with readable format
                configuration
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}");
            }
            else
            {
                // Production: Compact JSON for structured logging
                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .WriteTo.Console(new CompactJsonFormatter());
            }

            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.ConnectionString = appInsightsConnectionString;
                configuration.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }
        });
    }

    public static IApplicationBuilder UsePoJokerLogContext(this IApplicationBuilder app)
    {
        var hostEnvironment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        return app.Use(async (httpContext, next) =>
        {
            var userId = httpContext.User.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity.Name ?? "anonymous"
                : "anonymous";

            var sessionId = httpContext.Request.Headers.TryGetValue("X-Session-Id", out var headerSessionId)
                ? headerSessionId.ToString()
                : "unknown";

            var correlationId = httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var headerCorrelationId)
                ? headerCorrelationId.ToString()
                : httpContext.TraceIdentifier;

            httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("SessionId", sessionId))
            using (LogContext.PushProperty("Environment", hostEnvironment.EnvironmentName))
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next();
            }
        });
    }

    /// <summary>
    /// Adds Serilog request logging middleware configuration.
    /// </summary>
    public static IApplicationBuilder UsePoJokerRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? "unknown");

                var sessionId = httpContext.Request.Headers.TryGetValue("X-Session-Id", out var headerSessionId)
                    ? headerSessionId.ToString()
                    : "unknown";

                var correlationId = httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var headerCorrelationId)
                    ? headerCorrelationId.ToString()
                    : httpContext.TraceIdentifier;

                diagnosticContext.Set("SessionId", sessionId);
                diagnosticContext.Set("CorrelationId", correlationId);
                diagnosticContext.Set("Environment", httpContext.RequestServices.GetRequiredService<IHostEnvironment>().EnvironmentName);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name ?? "anonymous");
                }
                else
                {
                    diagnosticContext.Set("UserId", "anonymous");
                }
            };

            // Exclude health check endpoints from logging
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (httpContext.Request.Path.StartsWithSegments("/health") ||
                    httpContext.Request.Path.StartsWithSegments("/healthz"))
                {
                    return LogEventLevel.Verbose;
                }

                return ex != null ? LogEventLevel.Error :
                       elapsed > 1000 ? LogEventLevel.Warning :
                       LogEventLevel.Information;
            };
        });
    }
}
