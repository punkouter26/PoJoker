namespace Po.Joker.Contracts;

/// <summary>
/// Interface for audio effect playback service.
/// Abstracts audio operations to support multiple implementations (client, server, no-op).
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Initializes the audio service (loads Web Audio API context).
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Plays a drum roll sound effect.
    /// </summary>
    /// <param name="duration">Duration of the drum roll in seconds (default 2.0)</param>
    /// <param name="volume">Volume level (0.0 to 1.0, default 0.5)</param>
    Task PlayDrumRollAsync(double duration = 2.0, double volume = 0.5);

    /// <summary>
    /// Plays a sad trombone "wah-wah" sound effect.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0, default 0.6)</param>
    Task PlayTromboneAsync(double volume = 0.6);

    /// <summary>
    /// Plays a triumphant fanfare sound effect.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0, default 0.5)</param>
    Task PlayFanfareAsync(double volume = 0.5);

    /// <summary>
    /// Plays a cymbal crash sound effect.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0, default 0.4)</param>
    Task PlayCymbalAsync(double volume = 0.4);

    /// <summary>
    /// Determines if audio playback is supported in the current environment.
    /// </summary>
    /// <returns>True if audio is supported, false otherwise</returns>
    Task<bool> IsSupportedAsync();
}
