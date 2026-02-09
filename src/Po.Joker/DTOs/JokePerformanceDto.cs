using Po.Joker.Enums;

namespace Po.Joker.DTOs;

/// <summary>
/// Represents a complete joke performance (fetch + analysis cycle).
/// </summary>
public sealed record JokePerformanceDto
{
    /// <summary>
    /// Unique identifier for this performance.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Session identifier grouping related performances.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The joke that was performed.
    /// </summary>
    public required JokeDto Joke { get; init; }

    /// <summary>
    /// The AI analysis of the joke.
    /// </summary>
    public required JokeAnalysisDto Analysis { get; init; }

    /// <summary>
    /// Sequence number within the session (1-based).
    /// </summary>
    public int SequenceNumber { get; init; }

    /// <summary>
    /// Whether this was a triumph (AI guessed correctly).
    /// </summary>
    public bool IsTriumph => Analysis.IsTriumph;

    /// <summary>
    /// Timestamp when the performance started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Timestamp when the performance completed.
    /// </summary>
    public DateTimeOffset CompletedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Total duration of the performance in milliseconds.
    /// </summary>
    public long DurationMs => (long)(CompletedAt - StartedAt).TotalMilliseconds;

    /// <summary>
    /// State of the performance for UI display.
    /// </summary>
    public PerformanceState State { get; init; } = PerformanceState.Transitioning;
}
