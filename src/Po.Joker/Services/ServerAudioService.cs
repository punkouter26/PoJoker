using Po.Joker.Services;

namespace Po.Joker.Services;

/// <summary>
/// Server-side no-op implementation of IAudioService.
/// Audio only works on client-side via JavaScript interop.
/// </summary>
public sealed class ServerAudioService : IAudioService
{
    public Task InitializeAsync() => Task.CompletedTask;

    public Task PlayDrumRollAsync(double duration = 2.0, double volume = 0.5) => Task.CompletedTask;

    public Task PlayTromboneAsync(double volume = 0.6) => Task.CompletedTask;

    public Task PlayFanfareAsync(double volume = 0.5) => Task.CompletedTask;

    public Task PlayCymbalAsync(double volume = 0.4) => Task.CompletedTask;

    public Task<bool> IsSupportedAsync() => Task.FromResult(false);
}

/// <summary>
/// Server-side no-op implementation of ISpeechService.
/// Speech synthesis only works on client-side via JavaScript interop.
/// </summary>
public sealed class ServerSpeechService : ISpeechService
{
    public Task SpeakAsync(string text, double rate = 1.0, double pitch = 1.0) => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;

    public Task<bool> IsSupportedAsync() => Task.FromResult(false);

    public Task<bool> IsSpeakingAsync() => Task.FromResult(false);
}
