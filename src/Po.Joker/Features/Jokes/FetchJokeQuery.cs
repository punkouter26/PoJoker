using MediatR;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// MediatR query to fetch a joke from JokeAPI.
/// </summary>
/// <param name="SafeMode">When true, filters out inappropriate content.</param>
/// <param name="ExcludeIds">Optional joke IDs to exclude (prevents duplicates).</param>
public sealed record FetchJokeQuery(
    bool SafeMode = true,
    IEnumerable<int>? ExcludeIds = null) : IRequest<JokeDto>;
