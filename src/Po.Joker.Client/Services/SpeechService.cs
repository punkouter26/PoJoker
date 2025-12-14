using Microsoft.JSInterop;

namespace Po.Joker.Client.Services;

/// <summary>
/// Service for text-to-speech functionality using Web Speech API.
/// Provides a British male voice jester character for narration (FR-016).
/// </summary>
public interface ISpeechService
{
    /// <summary>
    /// Speak text with the jester voice.
    /// </summary>
    /// <param name="text">The text to speak.</param>
    /// <param name="rate">Speech rate (0.1 to 10, default 1.0).</param>
    /// <param name="pitch">Voice pitch (0 to 2, default 1.0).</param>
    Task SpeakAsync(string text, double rate = 1.0, double pitch = 1.0);

    /// <summary>
    /// Stop any ongoing speech.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Check if speech synthesis is supported.
    /// </summary>
    Task<bool> IsSupportedAsync();

    /// <summary>
    /// Check if currently speaking.
    /// </summary>
    Task<bool> IsSpeakingAsync();
}

/// <summary>
/// Implementation of ISpeechService using JavaScript interop.
/// </summary>
public sealed class SpeechService : ISpeechService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<SpeechService> _logger;

    public SpeechService(IJSRuntime jsRuntime, ILogger<SpeechService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task SpeakAsync(string text, double rate = 1.0, double pitch = 1.0)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerSpeech.speak", text, rate, pitch);
            _logger.LogDebug("Speaking: {Text}", text.Length > 50 ? text[..50] + "..." : text);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to speak text");
        }
    }

    public async Task StopAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("poJokerSpeech.stop");
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Failed to stop speech");
        }
    }

    public async Task<bool> IsSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("poJokerSpeech.isSupported");
        }
        catch (JSException)
        {
            return false;
        }
    }

    public async Task<bool> IsSpeakingAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("poJokerSpeech.isSpeaking");
        }
        catch (JSException)
        {
            return false;
        }
    }
}
