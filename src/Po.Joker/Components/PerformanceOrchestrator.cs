using System.Net.Http.Json;
using Po.Joker.Services;
using Po.Joker.DTOs;
using Po.Joker.Enums;

namespace Po.Joker.Components;

/// <summary>
/// Orchestrates the performance loop for the Jester Stage.
/// Separates business logic from UI component (SOLID: Single Responsibility).
/// </summary>
public class PerformanceOrchestrator : IDisposable
{
    private readonly HttpClient _http;
    private readonly ISpeechService _speechService;
    private readonly IAudioService _audioService;
    private CancellationTokenSource? _cts;

    public string SessionId { get; private set; }
    public PerformanceState CurrentState { get; private set; } = PerformanceState.Idle;
    public JokeDto? CurrentJoke { get; private set; }
    public JokeAnalysisDto? CurrentAnalysis { get; private set; }
    public int SessionTriumphs { get; private set; }
    public int SessionDefeats { get; private set; }
    public bool IsRunning { get; private set; }
    public List<int> SeenJokeIds { get; private set; } = [];

    // Configuration
    public bool SafeModeEnabled { get; set; } = true;
    public bool AudioEnabled { get; set; } = true;

    // Error handling state
    public bool ShowSpeechlessOverlay { get; private set; }
    public string SpeechlessMessage { get; private set; } = "The Court's content policy has silenced this jest.";
    public bool ShowNetworkOverlay { get; private set; }
    public string NetworkStatusText { get; private set; } = "Searching for a path to the server...";
    public bool IsRetrying { get; private set; }
    public int RetryCount { get; private set; }

    public event Action? StateChanged;

    public PerformanceOrchestrator(HttpClient http, ISpeechService speechService, IAudioService audioService)
    {
        _http = http;
        _speechService = speechService;
        _audioService = audioService;
        SessionId = Guid.NewGuid().ToString("N")[..8];
    }

    public async Task StartAsync()
    {
        IsRunning = true;
        _cts = new CancellationTokenSource();

        if (AudioEnabled)
        {
            await _audioService.InitializeAsync();
        }

        await RunPerformanceLoopAsync();
    }

    public async Task StopAsync()
    {
        IsRunning = false;
        _cts?.Cancel();
        CurrentState = PerformanceState.Idle;
        await _speechService.StopAsync();
        NotifyStateChanged();
    }

    public async Task ResumeSpeechlessAsync()
    {
        ShowSpeechlessOverlay = false;
        NotifyStateChanged();
    }

    public async Task RetryNetworkAsync()
    {
        IsRetrying = true;
        RetryCount++;
        NotifyStateChanged();

        await Task.Delay(1000);

        IsRetrying = false;
        ShowNetworkOverlay = false;
        NotifyStateChanged();
    }

    private async Task RunPerformanceLoopAsync()
    {
        while (IsRunning && !(_cts?.IsCancellationRequested ?? true))
        {
            try
            {
                // Act 1: Fetch Joke
                await FetchJokeAsync();
                if (!IsRunning || CurrentJoke == null) break;

                // Act 2: Show Setup
                await ShowSetupAsync();
                if (!IsRunning) break;

                // Act 3: AI Prediction
                await ShowAiGuessAsync();
                if (!IsRunning) break;

                // Act 4: Reveal Punchline
                await RevealPunchlineAsync();
                if (!IsRunning) break;

                // Act 5: Transition
                await TransitionAsync();
            }
            catch (HttpRequestException)
            {
                await HandleNetworkErrorAsync();
            }
            catch (Exception)
            {
                await Task.Delay(3000);
            }
        }
    }

    private async Task FetchJokeAsync()
    {
        CurrentState = PerformanceState.Fetching;
        NotifyStateChanged();

        var excludeIdsQuery = SeenJokeIds.Count > 0
            ? string.Join("", SeenJokeIds.TakeLast(50).Select(id => $"&excludeIds={id}"))
            : "";

        var response = await _http.GetAsync($"/api/jokes/fetch?safeMode={SafeModeEnabled}{excludeIdsQuery}");

        if (!response.IsSuccessStatusCode)
        {
            await Task.Delay(2000);
            return;
        }

        CurrentJoke = await response.Content.ReadFromJsonAsync<JokeDto>();
        if (CurrentJoke is not null)
        {
            SeenJokeIds.Add(CurrentJoke.Id);
        }
    }

    private async Task ShowSetupAsync()
    {
        CurrentState = PerformanceState.ShowingSetup;
        NotifyStateChanged();

        if (AudioEnabled && CurrentJoke is not null)
        {
            await _speechService.SpeakAsync(CurrentJoke.Setup, rate: 0.9, pitch: 1.1);
        }

        await Task.Delay(3000);
    }

    private async Task ShowAiGuessAsync()
    {
        CurrentState = PerformanceState.ShowingAiGuess;
        NotifyStateChanged();

        // Start drum roll
        var drumRollTask = AudioEnabled ? _audioService.PlayDrumRollAsync(2.0, 0.4) : Task.CompletedTask;

        // Send analyze request
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/jokes/analyze");
        request.Headers.Add("X-Session-Id", SessionId);
        request.Content = JsonContent.Create(CurrentJoke);
        var analysisResponse = await _http.SendAsync(request);

        // Check for content filter (451)
        if ((int)analysisResponse.StatusCode == 451)
        {
            SpeechlessMessage = "This jest was deemed too bold for the royal court!";
            ShowSpeechlessOverlay = true;
            NotifyStateChanged();

            while (ShowSpeechlessOverlay && IsRunning && !(_cts?.IsCancellationRequested ?? true))
            {
                await Task.Delay(500);
            }
            return;
        }

        if (analysisResponse.IsSuccessStatusCode)
        {
            CurrentAnalysis = await analysisResponse.Content.ReadFromJsonAsync<JokeAnalysisDto>();
        }

        await drumRollTask;

        // Narrate AI's guess
        if (AudioEnabled && CurrentAnalysis?.AiPunchline is not null)
        {
            await _speechService.SpeakAsync($"The Jester guesses: {CurrentAnalysis.AiPunchline}", rate: 0.9, pitch: 1.0);
        }

        NotifyStateChanged();
        await Task.Delay(2000);
    }

    private async Task RevealPunchlineAsync()
    {
        CurrentState = PerformanceState.RevealingPunchline;
        NotifyStateChanged();

        var isTriumph = CurrentAnalysis?.IsTriumph == true;
        if (isTriumph)
        {
            SessionTriumphs++;
            if (AudioEnabled)
            {
                await _audioService.PlayFanfareAsync(0.5);
            }
        }
        else
        {
            SessionDefeats++;
            if (AudioEnabled)
            {
                await _audioService.PlayTromboneAsync(0.5);
            }
        }

        // Narrate the actual punchline
        if (AudioEnabled && CurrentJoke?.Punchline is not null)
        {
            await Task.Delay(500);
            await _speechService.SpeakAsync($"The actual punchline is: {CurrentJoke.Punchline}", rate: 0.85, pitch: 1.0);
        }

        await Task.Delay(3000);
    }

    private async Task TransitionAsync()
    {
        CurrentState = PerformanceState.Transitioning;
        NotifyStateChanged();
        await Task.Delay(1000);
    }

    private async Task HandleNetworkErrorAsync()
    {
        NetworkStatusText = "The courier was lost on the road...";
        ShowNetworkOverlay = true;
        NotifyStateChanged();

        while (ShowNetworkOverlay && IsRunning && !(_cts?.IsCancellationRequested ?? true))
        {
            await Task.Delay(500);
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
