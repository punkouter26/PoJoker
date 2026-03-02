using Microsoft.JSInterop;
using Po.Joker.Contracts;

namespace Po.Joker.Services;

/// <summary>
/// Service for text-to-speech functionality using Web Speech API.
/// Provides a British male voice jester character for narration (FR-016).
/// </summary>
public class SpeechService(IJSRuntime jsRuntime, ILogger<SpeechService> logger) : ISpeechService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ILogger<SpeechService> _logger = logger;

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
