using Microsoft.JSInterop;
using Po.Joker.Contracts;

namespace Po.Joker.Services;

/// <summary>
/// Service for playing audio effects using Web Audio API.
/// Provides programmatic drum roll and trombone sounds (FR-017).
/// </summary>
public class AudioService(IJSRuntime jsRuntime, ILogger<AudioService> logger) : IAudioService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ILogger<AudioService> _logger = logger;
    private bool _initialized;

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerAudio.init");
            _initialized = true;
            _logger.LogDebug("Audio service initialized");
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to initialize audio");
        }
    }

    public async Task PlayDrumRollAsync(double duration = 2.0, double volume = 0.5)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerAudio.playDrumRoll", duration, volume);
            _logger.LogDebug("Playing drum roll (duration: {Duration}s)", duration);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to play drum roll");
        }
    }

    public async Task PlayTromboneAsync(double volume = 0.6)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerAudio.playTrombone", volume);
            _logger.LogDebug("Playing sad trombone");
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to play trombone");
        }
    }

    public async Task PlayFanfareAsync(double volume = 0.5)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerAudio.playFanfare", volume);
            _logger.LogDebug("Playing fanfare");
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to play fanfare");
        }
    }

    public async Task PlayCymbalAsync(double volume = 0.4)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerAudio.playCymbal", volume);
            _logger.LogDebug("Playing cymbal");
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to play cymbal");
        }
    }

    public async Task<bool> IsSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("poJokerAudio.isSupported");
        }
        catch (JSException)
        {
            return false;
        }
    }
}
