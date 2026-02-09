using MediatR;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Leaderboards;

/// <summary>
/// Handler for retrieving leaderboard entries with sorting and filtering.
/// </summary>
public sealed class GetLeaderboardHandler : IRequestHandler<GetLeaderboardQuery, IReadOnlyList<LeaderboardEntryDto>>
{
    private readonly IJokeStorageClient _storageClient;
    private readonly ILogger<GetLeaderboardHandler> _logger;

    public GetLeaderboardHandler(IJokeStorageClient storageClient, ILogger<GetLeaderboardHandler> logger)
    {
        _storageClient = storageClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting leaderboard: SortBy={SortBy}, Category={Category}, Top={Top}",
            request.SortBy, request.Category, request.Top);

        // Get entries from storage
        var entries = await _storageClient.GetLeaderboardAsync(request.Top * 2, cancellationToken);

        // Apply sorting based on the requested category
        var sorted = request.SortBy?.ToUpperInvariant() switch
        {
            "TRIUMPH" => entries.OrderByDescending(e => e.Triumphs),
            "CLEVERNESS" => entries.OrderByDescending(e => e.Score), // Score includes cleverness factor
            "RUDENESS" => entries.OrderByDescending(e => e.Score),
            "COMPLEXITY" => entries.OrderByDescending(e => e.Score),
            "DIFFICULTY" => entries.OrderByDescending(e => e.Score),
            _ => entries.OrderByDescending(e => e.Score)
        };

        // Apply limit and re-rank
        var result = sorted
            .Take(request.Top)
            .Select((entry, index) => entry with { Rank = index + 1 })
            .ToList();

        _logger.LogInformation("Retrieved {Count} leaderboard entries", result.Count);

        return result;
    }
}
