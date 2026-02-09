namespace Po.Joker.DTOs;

/// <summary>
/// Configuration settings for the passive comedy loop.
/// </summary>
public sealed record JesterSettingsDto
{
    /// <summary>
    /// Whether safe mode is enabled (filters NSFW content).
    /// Default: true per FR-007.
    /// </summary>
    public bool SafeMode { get; init; } = true;

    /// <summary>
    /// Delay between joke performances in seconds.
    /// Default: 15 seconds per FR-001.
    /// </summary>
    public int LoopIntervalSeconds { get; init; } = 15;

    /// <summary>
    /// Delay before revealing punchline in milliseconds.
    /// Default: 2000ms per SC-002.
    /// </summary>
    public int PunchlineDelayMs { get; init; } = 2000;

    /// <summary>
    /// Whether text-to-speech is enabled.
    /// </summary>
    public bool TtsEnabled { get; init; } = true;

    /// <summary>
    /// Speech rate for TTS (0.5 to 2.0).
    /// </summary>
    public double TtsRate { get; init; } = 1.0;

    /// <summary>
    /// Speech pitch for TTS (0.5 to 2.0).
    /// </summary>
    public double TtsPitch { get; init; } = 1.0;

    /// <summary>
    /// Joke categories to include (empty = all safe categories).
    /// </summary>
    public IReadOnlyList<string> Categories { get; init; } = [];

    /// <summary>
    /// Joke types to include: "twopart", "single", or both.
    /// </summary>
    public IReadOnlyList<string> JokeTypes { get; init; } = ["twopart"];
}
