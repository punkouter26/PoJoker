namespace Po.Joker.DTOs;

/// <summary>
/// Leaderboard entry for high scores display.
/// </summary>
public sealed record LeaderboardEntryDto
{
    /// <summary>
    /// Rank position on the leaderboard (1-based).
    /// </summary>
    public int Rank { get; init; }

    /// <summary>
    /// Session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Total jokes performed in the session.
    /// </summary>
    public int TotalJokes { get; init; }

    /// <summary>
    /// Number of AI triumphs.
    /// </summary>
    public int Triumphs { get; init; }

    /// <summary>
    /// Triumph rate as a percentage.
    /// </summary>
    public double TriumphRate { get; init; }

    /// <summary>
    /// Score calculated for ranking.
    /// Formula: (Triumphs * 100) + (TriumphRate * 10)
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// When the session was completed.
    /// </summary>
    public DateTimeOffset CompletedAt { get; init; }

    /// <summary>
    /// Whether this entry belongs to the current user's session.
    /// </summary>
    public bool IsCurrentSession { get; init; }
}
