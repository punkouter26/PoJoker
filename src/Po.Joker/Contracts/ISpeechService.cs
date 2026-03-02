namespace Po.Joker.Contracts;

/// <summary>
/// Interface for text-to-speech service.
/// Abstracts speech synthesis to support multiple implementations (client, server, no-op).
/// </summary>
public interface ISpeechService
{
    /// <summary>
    /// Synthesizes and speaks the provided text.
    /// </summary>
    /// <param name="text">Text to speak</param>
    /// <param name="rate">Speech rate multiplier (default 1.0, range 0.1 to 2.0)</param>
    /// <param name="pitch">Speech pitch multiplier (default 1.0, range 0.1 to 2.0)</param>
    Task SpeakAsync(string text, double rate = 1.0, double pitch = 1.0);

    /// <summary>
    /// Stops any currently ongoing speech synthesis.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Determines if speech synthesis is supported in the current environment.
    /// </summary>
    /// <returns>True if speech synthesis is supported, false otherwise</returns>
    Task<bool> IsSupportedAsync();

    /// <summary>
    /// Determines if speech is currently being synthesized.
    /// </summary>
    /// <returns>True if speech is currently playing, false otherwise</returns>
    Task<bool> IsSpeakingAsync();
}
