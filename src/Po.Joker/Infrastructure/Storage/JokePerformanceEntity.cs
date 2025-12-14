using Azure;
using Azure.Data.Tables;

namespace Po.Joker.Infrastructure.Storage;

/// <summary>
/// Table entity for persisting joke performance data.
/// PartitionKey: SessionId
/// RowKey: InvertedTimestamp_PerformanceId (for descending sort order)
/// </summary>
public sealed class JokePerformanceEntity : ITableEntity
{
    /// <summary>
    /// Session identifier (partitioning key).
    /// </summary>
    public required string PartitionKey { get; set; }

    /// <summary>
    /// Inverted timestamp + performance ID for ordering.
    /// </summary>
    public required string RowKey { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency.
    /// </summary>
    public ETag ETag { get; set; }

    /// <summary>
    /// Timestamp when the entity was last modified.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    // Performance identifiers
    public string PerformanceId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }

    // Joke data
    public int JokeId { get; set; }
    public string JokeCategory { get; set; } = string.Empty;
    public string JokeType { get; set; } = string.Empty;
    public string JokeSetup { get; set; } = string.Empty;
    public string JokePunchline { get; set; } = string.Empty;
    public string JokeText { get; set; } = string.Empty;
    public bool SafeMode { get; set; }

    // AI Analysis data
    public string AiPunchline { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public bool IsTriumph { get; set; }
    public double SimilarityScore { get; set; }
    public long AiLatencyMs { get; set; }

    // Timing data
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public long DurationMs { get; set; }

    // Flag data (stored as bool for efficient filtering)
    public bool FlagNsfw { get; set; }
    public bool FlagReligious { get; set; }
    public bool FlagPolitical { get; set; }
    public bool FlagRacist { get; set; }
    public bool FlagSexist { get; set; }
    public bool FlagExplicit { get; set; }

    // Rating data (US3 - AI Analysis)
    public int RatingCleverness { get; set; }
    public int RatingRudeness { get; set; }
    public int RatingComplexity { get; set; }
    public int RatingDifficulty { get; set; }
    public double RatingAverage { get; set; }
    public string RatingCommentary { get; set; } = string.Empty;
}
