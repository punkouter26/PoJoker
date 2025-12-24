using MediatR;
using Microsoft.AspNetCore.Mvc;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Diagnostics;

/// <summary>
/// API endpoints for system diagnostics.
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
