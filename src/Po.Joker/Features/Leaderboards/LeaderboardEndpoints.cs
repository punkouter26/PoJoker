using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Leaderboards;

/// <summary>
/// Minimal API endpoints for leaderboard operations.
/// </summary>
public static class LeaderboardEndpoints
{
    public static void MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leaderboard")
            .WithTags("Leaderboard");

        group.MapGet("/", GetLeaderboard)
            .WithName("GetLeaderboard")
            .WithSummary("Get top jokes by rating category")
            .Produces<IReadOnlyList<LeaderboardEntryDto>>();
    }

    /// <summary>
    /// GET /api/leaderboard - Get leaderboard sorted by category.
    /// </summary>
    private static async Task<Ok<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboard(
        IMediator mediator,
        string sortBy = "Triumph",
        string? category = null,
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        // Clamp top to valid range
        top = Math.Clamp(top, 1, 100);

        var query = new GetLeaderboardQuery(sortBy, category, top);
        var result = await mediator.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
