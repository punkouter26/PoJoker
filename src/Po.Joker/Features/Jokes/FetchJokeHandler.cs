using MediatR;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// MediatR handler for fetching jokes from JokeAPI.
/// Handles exclusion logic by retrying if a duplicate joke is returned.
/// </summary>
public sealed class FetchJokeHandler : IRequestHandler<FetchJokeQuery, JokeDto>
{
    private readonly IJokeApiClient _jokeApiClient;
    private readonly ILogger<FetchJokeHandler> _logger;
    private const int MaxRetries = 5;

    public FetchJokeHandler(IJokeApiClient jokeApiClient, ILogger<FetchJokeHandler> logger)
    {
        _jokeApiClient = jokeApiClient;
        _logger = logger;
    }

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

            // If no exclusions or joke is not in exclusion list, return it
            if (excludeSet.Count == 0 || !excludeSet.Contains(joke.Id))
            {
                _logger.LogInformation("Fetched joke Id={JokeId}, Category={Category}", joke.Id, joke.Category);
                return joke;
            }

            // Joke was in exclusion list, retry
            retryCount++;
            _logger.LogDebug("Joke Id={JokeId} is excluded, retrying ({Retry}/{MaxRetries})", joke.Id, retryCount, MaxRetries);
        }

        // After max retries, return whatever we get (may be duplicate)
        _logger.LogWarning("Could not find non-excluded joke after {MaxRetries} retries", MaxRetries);
        var fallbackJoke = await _jokeApiClient.FetchJokeAsync(request.SafeMode, null, cancellationToken);
        _logger.LogInformation("Returning fallback joke Id={JokeId}", fallbackJoke.Id);
        return fallbackJoke;
    }
}
