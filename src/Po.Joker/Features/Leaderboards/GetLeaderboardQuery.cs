using MediatR;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Leaderboards;

/// <summary>
/// Query to get leaderboard entries sorted by specified category.
/// </summary>
/// <param name="SortBy">Rating category to sort by (Cleverness, Rudeness, Complexity, Difficulty, Triumph).</param>
/// <param name="Category">Optional joke category filter.</param>
/// <param name="Top">Number of results to return (default 10, max 100).</param>
public sealed record GetLeaderboardQuery(
    string SortBy,
    string? Category,
    int Top = 10
) : IRequest<IReadOnlyList<LeaderboardEntryDto>>;
