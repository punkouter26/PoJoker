using Po.Joker.DTOs;

namespace Po.Joker.Contracts;

/// <summary>
/// Service contract for fetching jokes from external API.
/// </summary>
public interface IJokeService
{
    /// <summary>
    /// Fetches a random two-part joke from JokeAPI.
    /// </summary>
    /// <param name="safeMode">Whether to filter NSFW content.</param>
    /// <param name="excludeIds">Joke IDs to exclude (for duplicate prevention).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetched joke.</returns>
    Task<JokeDto> FetchJokeAsync(
        bool safeMode = true,
        IEnumerable<int>? excludeIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available joke categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available categories.</returns>
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
