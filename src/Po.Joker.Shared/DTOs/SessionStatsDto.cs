namespace Po.Joker.Shared.DTOs;

/// <summary>
/// Current session statistics for display.
/// </summary>
public sealed record SessionStatsDto
{
    /// <summary>
    /// Session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Total number of jokes performed this session.
    /// </summary>
    public int TotalJokes { get; init; }

    /// <summary>
    /// Number of AI triumphs (correct punchline guesses).
    /// </summary>
    public int Triumphs { get; init; }

    /// <summary>
    /// Triumph rate as a percentage (0-100).
    /// </summary>
    public double TriumphRate => TotalJokes > 0 ? Math.Round((double)Triumphs / TotalJokes * 100, 1) : 0;

    /// <summary>
    /// Average AI confidence across all predictions.
    /// </summary>
    public double AverageConfidence { get; init; }

    /// <summary>
    /// Average similarity score between AI predictions and actual punchlines.
    /// </summary>
    public double AverageSimilarity { get; init; }

    /// <summary>
    /// Average AI prediction latency in milliseconds.
    /// </summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>
    /// Session start time.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Session duration.
    /// </summary>
    public TimeSpan Duration => DateTimeOffset.UtcNow - StartedAt;

    /// <summary>
    /// Whether the loop is currently running.
    /// </summary>
    public bool IsRunning { get; init; }
}
