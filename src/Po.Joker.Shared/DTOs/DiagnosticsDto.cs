namespace Po.Joker.Shared.DTOs;

/// <summary>
/// Diagnostics information for application health monitoring.
/// </summary>
public sealed record DiagnosticsDto
{
    /// <summary>
    /// Application version.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Current environment (Development, Staging, Production).
    /// </summary>
    public required string Environment { get; init; }

    /// <summary>
    /// Timestamp of the diagnostics check.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Overall health status.
    /// </summary>
    public HealthStatus Status { get; init; } = HealthStatus.Healthy;

    /// <summary>
    /// Individual service health statuses.
    /// </summary>
    public IReadOnlyList<ServiceHealthDto> Services { get; init; } = [];

    /// <summary>
    /// Application uptime.
    /// </summary>
    public TimeSpan Uptime { get; init; }

    /// <summary>
    /// Total jokes served since startup.
    /// </summary>
    public long TotalJokesServed { get; init; }

    /// <summary>
    /// Total AI analyses performed since startup.
    /// </summary>
    public long TotalAnalyses { get; init; }

    /// <summary>
    /// Current AI triumph rate.
    /// </summary>
    public double TriumphRate { get; init; }
}

/// <summary>
/// Health status of an individual service dependency.
/// </summary>
public sealed record ServiceHealthDto
{
    /// <summary>
    /// Name of the service (e.g., "JokeAPI", "Azure OpenAI", "Table Storage").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Current health status.
    /// </summary>
    public HealthStatus Status { get; init; } = HealthStatus.Healthy;

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; init; }

    /// <summary>
    /// Additional status message if unhealthy.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Last successful check timestamp.
    /// </summary>
    public DateTimeOffset? LastChecked { get; init; }
}

/// <summary>
/// Health status enumeration.
/// </summary>
public enum HealthStatus
{
    /// <summary>Service is operating normally.</summary>
    Healthy,

    /// <summary>Service is operating with some degradation.</summary>
    Degraded,

    /// <summary>Service is not responding or failing.</summary>
    Unhealthy
}
