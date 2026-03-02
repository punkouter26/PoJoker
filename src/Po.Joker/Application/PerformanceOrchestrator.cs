using System.Net.Http.Json;
using Po.Joker.Contracts;
using Po.Joker.DTOs;
using Po.Joker.Enums;

namespace Po.Joker.Application;

/// <summary>
/// Orchestrates the performance loop for the Jester Stage.
/// Separates business logic from UI component (SOLID: Single Responsibility).
/// Implements IAsyncDisposable for proper async resource cleanup and cancellation.
/// </summary>
public class PerformanceOrchestrator : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly ISpeechService _speechService;
    private readonly IAudioService _audioService;
    private readonly PerformanceSettings _settings;
    private CancellationTokenSource? _cts;
    private Task? _performanceLoopTask;
    private bool _isRunning;

    public string SessionId { get; private set; } = Guid.NewGuid().ToString("N")[..8];
    public PerformanceState CurrentState { get; private set; } = PerformanceState.Idle;
    public JokeDto? CurrentJoke { get; private set; }
    public JokeAnalysisDto? CurrentAnalysis { get; private set; }
    public int SessionTriumphs { get; set; }
    public int SessionDefeats { get; set; }
    public bool IsRunning => _isRunning;
    public List<int> SeenJokeIds { get; set; } = [];

    // Demo mode: 10 jokes limit
    private const int MaxJokesPerShow = 10;
    public int JokesDisplayed { get; private set; }
    public bool IsShowComplete { get; private set; }
    public List<JokeAnalysisDto> TopJokes { get; private set; } = [];

    // Audio is always enabled in demo mode
    private bool AudioEnabled { get; } = true;

    // Error handling state
    public bool ShowSpeechlessOverlay { get; private set; }
    public string SpeechlessMessage { get; private set; } = "The Court's content policy has silenced this jest.";
    public bool ShowNetworkOverlay { get; private set; }
    public string NetworkStatusText { get; private set; } = "Searching for a path to the server...";
    public bool IsRetrying { get; private set; }
    public int RetryCount { get; private set; }

    public event Action? StateChanged;

    public PerformanceOrchestrator(
        HttpClient http,
        ISpeechService speechService,
        IAudioService audioService,
        PerformanceSettings? settings = null)
    {
        _http = http;
        _speechService = speechService;
        _audioService = audioService;
        _settings = settings ?? new PerformanceSettings();
        _settings.Validate();
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        _cts = new CancellationTokenSource();

        try
        {
            if (AudioEnabled)
            {
                await _audioService.InitializeAsync();
            }

            _performanceLoopTask = RunPerformanceLoopAsync(_cts.Token);
            await _performanceLoopTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when StopAsync() cancels the token
        }
        finally
        {
            _isRunning = false;
            NotifyStateChanged();
        }
    }

    public async Task StopAsync()
    {
        _isRunning = false;
        _cts?.Cancel();
        CurrentState = PerformanceState.Idle;
        
        try
        {
            await _speechService.StopAsync();
        }
        catch
        {
            // Ignore errors during stop
        }

        // Wait for performance loop to complete
        if (_performanceLoopTask is not null)
        {
            try
            {
                await _performanceLoopTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        NotifyStateChanged();
    }

    public async Task ResumeSpeechlessAsync()
    {
        ShowSpeechlessOverlay = false;
        NotifyStateChanged();
        await Task.CompletedTask;
    }

    public void ResetForNewShow()
    {
        CurrentState = PerformanceState.Idle;
        CurrentJoke = null;
        CurrentAnalysis = null;
        JokesDisplayed = 0;
        IsShowComplete = false;
        TopJokes.Clear();
        ShowSpeechlessOverlay = false;
        ShowNetworkOverlay = false;
        RetryCount = 0;
        NotifyStateChanged();
    }

    public async Task RetryNetworkAsync()
    {
        IsRetrying = true;
        RetryCount++;
        NotifyStateChanged();

        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_settings.RetryDelayMilliseconds), _cts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Ignore if cancelled during retry delay
        }

        IsRetrying = false;
        ShowNetworkOverlay = false;
        NotifyStateChanged();
    }

    private async Task RunPerformanceLoopAsync(CancellationToken cancellationToken)
    {
        JokesDisplayed = 0;
        IsShowComplete = false;
        TopJokes.Clear();

        while (!cancellationToken.IsCancellationRequested && JokesDisplayed < MaxJokesPerShow)
        {
            try
            {
                // Act 1: Fetch Joke
                await FetchJokeAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested || CurrentJoke == null) break;

                // Act 2: Show Setup
                await ShowSetupAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;

                // Act 3: AI Prediction
                await ShowAiGuessAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;

                // Act 4: Reveal Punchline
                await RevealPunchlineAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;

                // Track this joke in top jokes list
                JokesDisplayed++;
                if (CurrentAnalysis is not null)
                {
                    TopJokes.Add(CurrentAnalysis);
                }

                // Act 5: Transition
                if (JokesDisplayed < MaxJokesPerShow)
                {
                    await TransitionAsync(cancellationToken);
                }
            }
            catch (HttpRequestException)
            {
                await HandleNetworkErrorAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected during stop
                throw;
            }
            catch (Exception)
            {
                try
                {
                    await Task.Delay(3000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw; // Propagate cancellation
                }
            }
        }

        // Show completion screen when 10 jokes delivered
        if (JokesDisplayed >= MaxJokesPerShow && !cancellationToken.IsCancellationRequested)
        {
            IsShowComplete = true;
            CurrentState = PerformanceState.Complete;
            NotifyStateChanged();
        }
    }

    private async Task FetchJokeAsync(CancellationToken cancellationToken)
    {
        CurrentState = PerformanceState.Fetching;
        NotifyStateChanged();

        var excludeIdsQuery = SeenJokeIds.Count > 0
            ? string.Join("", SeenJokeIds.TakeLast(50).Select(id => $"&excludeIds={id}"))
            : "";

        try
        {
            var response = await _http.GetAsync($"/api/jokes/fetch?safeMode=false{excludeIdsQuery}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_settings.RetryDelayMilliseconds), cancellationToken);
                return;
            }

            CurrentJoke = await response.Content.ReadFromJsonAsync<JokeDto>(cancellationToken: cancellationToken);
            if (CurrentJoke is not null)
            {
                SeenJokeIds.Add(CurrentJoke.Id);
            }
        }
        catch (HttpRequestException)
        {
            throw; // Let caller handle network errors
        }
    }

    private async Task ShowSetupAsync(CancellationToken cancellationToken)
    {
        CurrentState = PerformanceState.ShowingSetup;
        NotifyStateChanged();

        if (AudioEnabled && CurrentJoke is not null)
        {
            await _speechService.SpeakAsync(CurrentJoke.Setup, rate: 0.9, pitch: 1.1);
        }

        await Task.Delay(TimeSpan.FromSeconds(_settings.SetupDurationSeconds), cancellationToken);
    }

    private async Task ShowAiGuessAsync(CancellationToken cancellationToken)
    {
        CurrentState = PerformanceState.ShowingAiGuess;
        NotifyStateChanged();

        // Start drum roll
        var drumRollTask = AudioEnabled ? _audioService.PlayDrumRollAsync(2.0, 0.4) : Task.CompletedTask;

        try
        {
            // Send analyze request with cancellation token
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/jokes/analyze");
            request.Headers.Add("X-Session-Id", SessionId);
            request.Content = JsonContent.Create(CurrentJoke);
            var analysisResponse = await _http.SendAsync(request, cancellationToken);

            // Check for content filter (451)
            if ((int)analysisResponse.StatusCode == 451)
            {
                SpeechlessMessage = "This jest was deemed too bold for the royal court!";
                ShowSpeechlessOverlay = true;
                NotifyStateChanged();

                // Wait for user to resume, checking cancellation
                while (ShowSpeechlessOverlay && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(_settings.UserInteractionPollMilliseconds), cancellationToken);
                }
                return;
            }

            if (analysisResponse.IsSuccessStatusCode)
            {
                CurrentAnalysis = await analysisResponse.Content.ReadFromJsonAsync<JokeAnalysisDto>(cancellationToken: cancellationToken);
            }

            await drumRollTask;

            // Narrate AI's guess
            if (AudioEnabled && CurrentAnalysis?.AiPunchline is not null)
            {
                await _speechService.SpeakAsync($"The Jester guesses: {CurrentAnalysis.AiPunchline}", rate: 0.9, pitch: 1.0);
            }

            NotifyStateChanged();
            await Task.Delay(TimeSpan.FromSeconds(_settings.PredictionDurationSeconds), cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw; // Let caller handle network errors
        }
    }

    private async Task RevealPunchlineAsync(CancellationToken cancellationToken)
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
            await Task.Delay(TimeSpan.FromSeconds(_settings.PunchlineDelaySeconds), cancellationToken);
            await _speechService.SpeakAsync($"The actual punchline is: {CurrentJoke.Punchline}", rate: 0.85, pitch: 1.0);
        }

        await Task.Delay(TimeSpan.FromSeconds(_settings.PunchlineDurationSeconds), cancellationToken);
    }

    private async Task TransitionAsync(CancellationToken cancellationToken)
    {
        CurrentState = PerformanceState.Transitioning;
        NotifyStateChanged();
        await Task.Delay(TimeSpan.FromSeconds(_settings.TransitionDurationSeconds), cancellationToken);
    }

    private async Task HandleNetworkErrorAsync(CancellationToken cancellationToken)
    {
        NetworkStatusText = "The courier was lost on the road...";
        ShowNetworkOverlay = true;
        NotifyStateChanged();

        // Wait for user to retry or cancellation
        while (ShowNetworkOverlay && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_settings.UserInteractionPollMilliseconds), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break; // Exit loop on cancellation
            }
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _performanceLoopTask?.Wait(TimeSpan.FromSeconds(5));
    }
}
