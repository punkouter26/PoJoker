namespace Po.Joker.DTOs;

/// <summary>
/// Represents an AI punchline analysis result.
/// </summary>
public sealed record JokeAnalysisDto
{
    /// <summary>
    /// Unique identifier for this analysis.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The original joke that was analyzed.
    /// </summary>
    public required JokeDto OriginalJoke { get; init; }

    /// <summary>
    /// The AI's predicted punchline.
    /// </summary>
    public required string AiPunchline { get; init; }

    /// <summary>
    /// The AI's confidence level in its prediction (0.0 - 1.0).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Whether the AI's prediction matched the actual punchline.
    /// </summary>
    public bool IsTriumph { get; init; }

    /// <summary>
    /// Similarity score between AI prediction and actual punchline (0.0 - 1.0).
    /// </summary>
    public double SimilarityScore { get; init; }

    /// <summary>
    /// Time taken for AI to generate the prediction in milliseconds.
    /// </summary>
    public long LatencyMs { get; init; }

    /// <summary>
    /// Timestamp when the analysis was performed.
    /// </summary>
    public DateTimeOffset AnalyzedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// AI rating of the joke across multiple dimensions (Cleverness, Rudeness, Complexity, Difficulty).
    /// </summary>
    public JokeRatingDto? Rating { get; init; }
}
