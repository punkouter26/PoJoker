namespace Po.Joker.Shared.DTOs;

/// <summary>
/// Represents a joke with setup and punchline.
/// Maps to JokeAPI response structure.
/// </summary>
public sealed record JokeDto
{
    /// <summary>
    /// Unique identifier for the joke from JokeAPI.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Category of the joke (e.g., "Programming", "Pun", "Dark").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Type of joke: "twopart" (setup + punchline) or "single" (one-liner).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The setup part of a two-part joke.
    /// Empty for single-type jokes.
    /// </summary>
    public string Setup { get; init; } = string.Empty;

    /// <summary>
    /// The punchline of a two-part joke.
    /// Empty for single-type jokes.
    /// </summary>
    public string Punchline { get; init; } = string.Empty;

    /// <summary>
    /// The complete joke text for single-type jokes.
    /// Empty for twopart jokes.
    /// </summary>
    public string Joke { get; init; } = string.Empty;

    /// <summary>
    /// Content flags indicating potentially sensitive content.
    /// </summary>
    public JokeFlags Flags { get; init; } = new();

    /// <summary>
    /// Whether safe mode was enabled when this joke was fetched.
    /// </summary>
    public bool SafeMode { get; init; }

    /// <summary>
    /// Gets the display text - either the setup or the single joke text.
    /// </summary>
    public string DisplayText => Type == "twopart" ? Setup : Joke;

    /// <summary>
    /// Gets the full joke text for display.
    /// </summary>
    public string FullText => Type == "twopart" ? $"{Setup}\n{Punchline}" : Joke;
}

/// <summary>
/// Content flags from JokeAPI.
/// </summary>
public sealed record JokeFlags
{
    public bool Nsfw { get; init; }
    public bool Religious { get; init; }
    public bool Political { get; init; }
    public bool Racist { get; init; }
    public bool Sexist { get; init; }
    public bool Explicit { get; init; }
}
