using Po.Joker.Contracts;

namespace Po.Joker.Services;

/// <summary>
/// Server-side no-op adapter for audio service.
/// Audio only works on client-side via JavaScript interop.
/// Uses adapter pattern (composition) instead of inheritance for LSP compliance.
/// </summary>
public sealed class NullAudioService(ILogger<NullAudioService> logger) : IAudioService
{
    public async Task InitializeAsync()
    {
        logger.LogDebug("Audio service (null) initialized");
        await Task.CompletedTask;
    }

    public async Task PlayDrumRollAsync(double duration = 2.0, double volume = 0.5)
    {
        logger.LogDebug("Audio (null) playing drum roll (duration: {Duration}s)", duration);
        await Task.CompletedTask;
    }

    public async Task PlayTromboneAsync(double volume = 0.6)
    {
        logger.LogDebug("Audio (null) playing trombone");
        await Task.CompletedTask;
    }

    public async Task PlayFanfareAsync(double volume = 0.5)
    {
        logger.LogDebug("Audio (null) playing fanfare");
        await Task.CompletedTask;
    }

    public async Task PlayCymbalAsync(double volume = 0.4)
    {
        logger.LogDebug("Audio (null) playing cymbal");
        await Task.CompletedTask;
    }

    public async Task<bool> IsSupportedAsync()
    {
        logger.LogDebug("Audio (null) is not supported");
        return await Task.FromResult(false);
    }
}

/// <summary>
/// Server-side no-op adapter for speech service.
/// Speech synthesis only works on client-side via JavaScript interop.
/// Uses adapter pattern (composition) instead of inheritance for LSP compliance.
/// </summary>
public sealed class NullSpeechService(ILogger<NullSpeechService> logger) : ISpeechService
{
    public async Task SpeakAsync(string text, double rate = 1.0, double pitch = 1.0)
    {
        logger.LogDebug("Speech (null) would speak: {Text}", text.Length > 50 ? text[..50] + "..." : text);
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        logger.LogDebug("Speech (null) stop requested");
        await Task.CompletedTask;
    }

    public async Task<bool> IsSupportedAsync()
    {
        logger.LogDebug("Speech (null) is not supported");
        return await Task.FromResult(false);
    }

    public async Task<bool> IsSpeakingAsync()
    {
        logger.LogDebug("Speech (null) is not speaking");
        return await Task.FromResult(false);
    }
}
