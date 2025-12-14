using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

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
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Po.Joker")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

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

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name ?? "anonymous");
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
