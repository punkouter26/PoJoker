using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Analysis;

/// <summary>
/// API endpoints for joke analysis operations.
/// </summary>
public static class AnalysisEndpoints
{
    public static IEndpointRouteBuilder MapAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jokes")
            .WithTags("Analysis");

        group.MapPost("/analyze", AnalyzeJoke)
            .WithName("AnalyzeJoke")
            .WithSummary("Analyze a joke using AI to predict the punchline")
            .Produces<JokeAnalysisDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<IResult> AnalyzeJoke(
        [FromBody] JokeDto joke,
        [FromHeader(Name = "X-Session-Id")] string? sessionId,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<JokeDto> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(joke, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Use provided session ID or generate a new one
        sessionId ??= Guid.NewGuid().ToString("N")[..8];

        var command = new AnalyzeJokeCommand(joke, sessionId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
