using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// Interface for JokeAPI client operations.
/// Fetches two-part jokes with optional safe mode and duplicate filtering.
/// </summary>
public interface IJokeApiClient
{
    /// <summary>
    /// Fetches a random two-part joke from JokeAPI.
    /// </summary>
    /// <param name="safeMode">When true, filters out nsfw, religious, political, racist, sexist content.</param>
    /// <param name="excludeIds">Optional joke IDs to exclude (prevents duplicates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A two-part joke DTO.</returns>
    Task<JokeDto> FetchJokeAsync(
        bool safeMode = true,
        IEnumerable<int>? excludeIds = null,
        CancellationToken cancellationToken = default);
}
