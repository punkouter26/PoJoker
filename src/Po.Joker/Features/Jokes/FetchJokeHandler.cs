using MediatR;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// MediatR handler for fetching jokes from JokeAPI.
/// Handles exclusion logic by retrying if a duplicate joke is returned.
/// Validates joke data to ensure Setup and Punchline are non-empty.
/// </summary>
public sealed class FetchJokeHandler(IJokeApiClient jokeApiClient, ILogger<FetchJokeHandler> logger) : IRequestHandler<FetchJokeQuery, JokeDto>
{
    private readonly IJokeApiClient _jokeApiClient = jokeApiClient;
    private readonly ILogger<FetchJokeHandler> _logger = logger;
    private const int MaxRetries = 5;

    public async Task<JokeDto> Handle(FetchJokeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching joke with SafeMode={SafeMode}", request.SafeMode);

        var excludeSet = request.ExcludeIds?.ToHashSet() ?? [];
        var retryCount = 0;

        while (retryCount < MaxRetries)
        {
            var joke = await _jokeApiClient.FetchJokeAsync(
                request.SafeMode,
                request.ExcludeIds,
                cancellationToken);

            // Validate joke has required fields
            if (!IsValidJoke(joke))
            {
                _logger.LogWarning(
                    "Fetched invalid joke Id={JokeId} (missing Setup or Punchline), excluding and retrying",
                    joke.Id);
                excludeSet.Add(joke.Id);
                retryCount++;
                continue;
            }

            // If no exclusions or joke is not in exclusion list, return it
            if (excludeSet.Count == 0 || !excludeSet.Contains(joke.Id))
            {
                _logger.LogInformation(
                    "Fetched valid joke Id={JokeId}, Category={Category}, SetupLength={SetupLength}, PunchlineLength={PunchlineLength}",
                    joke.Id, joke.Category, joke.Setup?.Length ?? 0, joke.Punchline?.Length ?? 0);
                return joke;
            }

            // Joke was in exclusion list, retry
            retryCount++;
            _logger.LogDebug("Joke Id={JokeId} is excluded, retrying ({Retry}/{MaxRetries})", joke.Id, retryCount, MaxRetries);
        }

        // After max retries, return whatever we get (may be duplicate)
        _logger.LogWarning("Could not find non-excluded valid joke after {MaxRetries} retries, returning fallback", MaxRetries);
        var fallbackJoke = await _jokeApiClient.FetchJokeAsync(request.SafeMode, null, cancellationToken);
        
        if (!IsValidJoke(fallbackJoke))
        {
            _logger.LogError("Fallback joke is also invalid, creating minimal joke to prevent null reference");
            // Create a minimal valid joke to avoid null reference errors downstream
            fallbackJoke = new JokeDto
            {
                Id = 0,
                Category = "Programming",
                Type = "single",
                Setup = "The AI was silent.",
                Punchline = "It had nothing to add.",
                Flags = new JokeFlags()
            };
        }

        _logger.LogInformation("Returning fallback joke Id={JokeId}", fallbackJoke.Id);
        return fallbackJoke;
    }

    /// <summary>
    /// Validates that a joke has required non-empty fields.
    /// </summary>
    private static bool IsValidJoke(JokeDto joke)
    {
        if (joke == null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(joke.Setup) &&
               !string.IsNullOrWhiteSpace(joke.Punchline);
    }
}
