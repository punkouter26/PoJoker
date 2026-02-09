using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Po.Joker.Infrastructure.Telemetry;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Diagnostics;

/// <summary>
/// Handler that aggregates all health checks and metrics for diagnostics.
/// </summary>
public sealed class GetDiagnosticsHandler : IRequestHandler<GetDiagnosticsQuery, DiagnosticsDto>
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GetDiagnosticsHandler> _logger;
    private static readonly DateTimeOffset _startupTime = DateTimeOffset.UtcNow;

    public GetDiagnosticsHandler(
        HealthCheckService healthCheckService,
        IHostEnvironment environment,
        ILogger<GetDiagnosticsHandler> logger)
    {
        _healthCheckService = healthCheckService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<DiagnosticsDto> Handle(GetDiagnosticsQuery request, CancellationToken cancellationToken)
    {
        using var activity = OpenTelemetryConfig.ActivitySource.StartActivity("Diagnostics.GetHealth");

        var stopwatch = Stopwatch.StartNew();
        var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation(
            "Health check completed in {Duration}ms with status {Status}",
            stopwatch.ElapsedMilliseconds,
            healthReport.Status);

        var services = healthReport.Entries.Select(entry => new ServiceHealthDto
        {
            Name = entry.Key,
            Status = MapHealthStatus(entry.Value.Status),
            ResponseTimeMs = entry.Value.Duration.Milliseconds,
            Message = entry.Value.Description ?? entry.Value.Exception?.Message,
            LastChecked = DateTimeOffset.UtcNow
        }).ToList();

        return new DiagnosticsDto
        {
            Version = GetVersion(),
            Environment = _environment.EnvironmentName,
            Timestamp = DateTimeOffset.UtcNow,
            Status = MapHealthStatus(healthReport.Status),
            Services = services,
            Uptime = DateTimeOffset.UtcNow - _startupTime,
            TotalJokesServed = GetJokesServedCount(),
            TotalAnalyses = GetAnalysesCount(),
            TriumphRate = GetTriumphRate()
        };
    }

    private static DTOs.HealthStatus MapHealthStatus(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus status)
    {
        return status switch
        {
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy => DTOs.HealthStatus.Healthy,
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded => DTOs.HealthStatus.Degraded,
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy => DTOs.HealthStatus.Unhealthy,
            _ => DTOs.HealthStatus.Unhealthy
        };
    }

    private static string GetVersion()
    {
        var assembly = typeof(GetDiagnosticsHandler).Assembly;
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }

    private static long GetJokesServedCount()
    {
        // TODO: Integrate with actual metrics counter when available
        return 0;
    }

    private static long GetAnalysesCount()
    {
        // TODO: Integrate with actual metrics counter when available
        return 0;
    }

    private static double GetTriumphRate()
    {
        // TODO: Integrate with actual triumph rate calculation when available
        return 0.0;
    }
}
