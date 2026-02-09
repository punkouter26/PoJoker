using Microsoft.JSInterop;

namespace Po.Joker.Services;

/// <summary>
/// Service for managing performance sessions using browser localStorage.
/// Tracks seen joke IDs to prevent duplicates and session statistics.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Gets or creates the current session ID.
    /// </summary>
    Task<string> GetSessionIdAsync();

    /// <summary>
    /// Adds a joke ID to the seen jokes list.
    /// </summary>
    Task AddSeenJokeAsync(int jokeId);

    /// <summary>
    /// Gets all seen joke IDs for the current session.
    /// </summary>
    Task<IReadOnlyList<int>> GetSeenJokeIdsAsync();

    /// <summary>
    /// Clears the current session and starts a new one.
    /// </summary>
    Task ClearSessionAsync();

    /// <summary>
    /// Increments the triumph count.
    /// </summary>
    Task IncrementTriumphsAsync();

    /// <summary>
    /// Increments the defeat count.
    /// </summary>
    Task IncrementDefeatsAsync();

    /// <summary>
    /// Gets the current session statistics.
    /// </summary>
    Task<SessionStats> GetSessionStatsAsync();
}

/// <summary>
/// Session statistics for the current performance session.
/// </summary>
public record SessionStats(
    string SessionId,
    int JokesPerformed,
    int Triumphs,
    int Defeats,
    DateTimeOffset StartedAt);

/// <summary>
/// Implementation of ISessionService using browser localStorage via JS interop.
/// </summary>
public sealed class SessionService : ISessionService
{
    private readonly IJSRuntime _jsRuntime;
    private const string SessionIdKey = "pojoker_session_id";
    private const string SeenJokesKey = "pojoker_seen_jokes";
    private const string TriumphsKey = "pojoker_triumphs";
    private const string DefeatsKey = "pojoker_defeats";
    private const string StartedAtKey = "pojoker_started_at";

    public SessionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetSessionIdAsync()
    {
        var sessionId = await GetLocalStorageItemAsync(SessionIdKey);
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString("N")[..8];
            await SetLocalStorageItemAsync(SessionIdKey, sessionId);
            await SetLocalStorageItemAsync(StartedAtKey, DateTimeOffset.UtcNow.ToString("O"));
        }
        return sessionId;
    }

    public async Task AddSeenJokeAsync(int jokeId)
    {
        var seenJokes = await GetSeenJokeIdsAsync();
        var newList = seenJokes.Append(jokeId).Distinct().ToList();
        await SetLocalStorageItemAsync(SeenJokesKey, string.Join(",", newList));
    }

    public async Task<IReadOnlyList<int>> GetSeenJokeIdsAsync()
    {
        var seenJokesJson = await GetLocalStorageItemAsync(SeenJokesKey);
        if (string.IsNullOrEmpty(seenJokesJson))
        {
            return [];
        }

        return seenJokesJson
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();
    }

    public async Task ClearSessionAsync()
    {
        await RemoveLocalStorageItemAsync(SessionIdKey);
        await RemoveLocalStorageItemAsync(SeenJokesKey);
        await RemoveLocalStorageItemAsync(TriumphsKey);
        await RemoveLocalStorageItemAsync(DefeatsKey);
        await RemoveLocalStorageItemAsync(StartedAtKey);
    }

    public async Task IncrementTriumphsAsync()
    {
        var current = await GetIntAsync(TriumphsKey);
        await SetLocalStorageItemAsync(TriumphsKey, (current + 1).ToString());
    }

    public async Task IncrementDefeatsAsync()
    {
        var current = await GetIntAsync(DefeatsKey);
        await SetLocalStorageItemAsync(DefeatsKey, (current + 1).ToString());
    }

    public async Task<SessionStats> GetSessionStatsAsync()
    {
        var sessionId = await GetSessionIdAsync();
        var triumphs = await GetIntAsync(TriumphsKey);
        var defeats = await GetIntAsync(DefeatsKey);
        var startedAtStr = await GetLocalStorageItemAsync(StartedAtKey);
        var startedAt = DateTimeOffset.TryParse(startedAtStr, out var dt)
            ? dt
            : DateTimeOffset.UtcNow;

        return new SessionStats(
            sessionId,
            triumphs + defeats,
            triumphs,
            defeats,
            startedAt);
    }

    private async Task<int> GetIntAsync(string key)
    {
        var value = await GetLocalStorageItemAsync(key);
        return int.TryParse(value, out var result) ? result : 0;
    }

    private async Task<string?> GetLocalStorageItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    private async Task SetLocalStorageItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch
        {
            // Ignore storage errors in SSR
        }
    }

    private async Task RemoveLocalStorageItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Ignore storage errors in SSR
        }
    }
}
