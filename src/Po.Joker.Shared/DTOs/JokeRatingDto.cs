namespace Po.Joker.Shared.DTOs;

/// <summary>
/// AI rating of a joke across multiple dimensions.
/// </summary>
public sealed record JokeRatingDto
{
    /// <summary>
    /// Cleverness score (1-10). How witty and ingenious is the wordplay or concept?
    /// </summary>
    public int Cleverness { get; init; }

    /// <summary>
    /// Rudeness score (1-10). How inappropriate or edgy is the humor?
    /// </summary>
    public int Rudeness { get; init; }

    /// <summary>
    /// Complexity score (1-10). How complex is the joke structure or concept?
    /// </summary>
    public int Complexity { get; init; }

    /// <summary>
    /// Difficulty score (1-10). How hard is it to predict the punchline?
    /// </summary>
    public int Difficulty { get; init; }

    /// <summary>
    /// AI Jester's personality-driven commentary on the joke.
    /// </summary>
    public string Commentary { get; init; } = string.Empty;

    /// <summary>
    /// Average of all scores.
    /// </summary>
    public double Average => (Cleverness + Rudeness + Complexity + Difficulty) / 4.0;
}
