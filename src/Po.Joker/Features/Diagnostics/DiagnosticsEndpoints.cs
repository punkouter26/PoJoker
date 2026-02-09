using MediatR;
using Microsoft.AspNetCore.Mvc;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Diagnostics;

/// <summary>
/// API endpoints for system diagnostics.
/// Maps both /api/diagnostics (health aggregation) and /diag (config/secrets inspector).
/// </summary>
public static class DiagnosticsEndpoints
{
    private static readonly DateTimeOffset _startupTime = DateTimeOffset.UtcNow;

    public static void MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diagnostics")
            .WithTags("Diagnostics");

        group.MapGet("/", GetDiagnostics)
            .WithName("GetDiagnostics")
            .WithSummary("Gets system diagnostics and health status")
            .Produces<DiagnosticsDto>(200)
            .Produces<DiagnosticsDto>(503);

        // /api/diag — exposes all configuration, connection strings, keys, and secrets with masked values
        app.MapGet("/api/diag", GetDiagConfig)
            .WithName("GetDiagConfig")
            .WithTags("Diagnostics")
            .WithSummary("Exposes all configuration values with masked secrets for debugging")
            .Produces<DiagConfigDto>(200);
    }

    /// <summary>
    /// GET /api/diag — Iterates all configuration providers and returns every key/value pair
    /// with the middle portion of each value masked for security.
    /// </summary>
    private static IResult GetDiagConfig(
        [FromServices] IConfiguration configuration,
        [FromServices] IHostEnvironment environment)
    {
        var entries = new List<DiagConfigEntryDto>();

        // Flatten all configuration key/value pairs from every provider
        if (configuration is IConfigurationRoot configRoot)
        {
            foreach (var provider in configRoot.Providers)
            {
                var providerName = provider.GetType().Name;
                RecurseChildren(provider, providerName, null, entries);
            }
        }

        // Deduplicate: later providers override earlier ones; keep last occurrence per key
        var deduped = entries
            .GroupBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var dto = new DiagConfigDto
        {
            Environment = environment.EnvironmentName,
            Timestamp = DateTimeOffset.UtcNow,
            Entries = deduped
        };

        return Results.Json(dto);
    }

    /// <summary>
    /// Recursively enumerates all keys from a configuration provider.
    /// </summary>
    private static void RecurseChildren(
        IConfigurationProvider provider,
        string providerName,
        string? parentPath,
        List<DiagConfigEntryDto> entries)
    {
        var keys = provider.GetChildKeys([], parentPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var key in keys)
        {
            var fullKey = parentPath is null ? key : $"{parentPath}:{key}";
            if (provider.TryGet(fullKey, out var value))
            {
                entries.Add(new DiagConfigEntryDto
                {
                    Key = fullKey,
                    Value = ConfigMasker.Mask(value),
                    Source = providerName
                });
            }

            // Recurse into child keys
            RecurseChildren(provider, providerName, fullKey, entries);
        }
    }

    private static async Task<IResult> GetDiagnostics(
        [FromServices] IMediator mediator,
        [FromServices] IHostEnvironment environment,
        [FromServices] ILogger<GetDiagnosticsQuery> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetDiagnosticsQuery(), cancellationToken);
            var statusCode = result.Status == HealthStatus.Healthy ? 200 : 503;
            return Results.Json(result, statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve diagnostics");
            
            // Return a valid DiagnosticsDto even when health checks fail
            var errorResponse = new DiagnosticsDto
            {
                Version = GetVersion(),
                Environment = environment.EnvironmentName,
                Timestamp = DateTimeOffset.UtcNow,
                Status = HealthStatus.Unhealthy,
                Services = new List<ServiceHealthDto>
                {
                    new ServiceHealthDto
                    {
                        Name = "System",
                        Status = HealthStatus.Unhealthy,
                        Message = ex.Message,
                        LastChecked = DateTimeOffset.UtcNow
                    }
                },
                Uptime = DateTimeOffset.UtcNow - _startupTime,
                TotalJokesServed = 0,
                TotalAnalyses = 0,
                TriumphRate = 0.0
            };
            
            return Results.Json(errorResponse, statusCode: 503);
        }
    }

    private static string GetVersion()
    {
        var assembly = typeof(DiagnosticsEndpoints).Assembly;
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}
