using MediatR;
using Microsoft.AspNetCore.Mvc;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// API endpoints for joke operations.
/// </summary>
public static class JokesEndpoints
{
    public static IEndpointRouteBuilder MapJokesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jokes")
            .WithTags("Jokes");

        group.MapGet("/fetch", FetchJoke)
            .WithName("FetchJoke")
            .WithSummary("Fetch a random two-part joke from JokeAPI")
            .Produces<JokeDto>()
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<IResult> FetchJoke(
        [FromQuery] bool safeMode,
        [FromQuery] int[]? excludeIds,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new FetchJokeQuery(safeMode, excludeIds);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
