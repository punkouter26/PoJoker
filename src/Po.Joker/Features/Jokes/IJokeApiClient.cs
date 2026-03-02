using Po.Joker.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// Abstraction over the JokeAPI external HTTP service.
/// </summary>
public interface IJokeApiClient
{
    Task<JokeDto> FetchJokeAsync(
        bool safeMode = true,
        IEnumerable<int>? excludeIds = null,
        CancellationToken cancellationToken = default);
}
