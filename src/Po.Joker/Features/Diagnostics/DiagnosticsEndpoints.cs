using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Po.Joker.Features.Diagnostics;

/// <summary>
/// API endpoints for system diagnostics.
/// </summary>
public static class DiagnosticsEndpoints
{
    public static void MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diagnostics")
            .WithTags("Diagnostics");

        group.MapGet("/", GetDiagnostics)
            .WithName("GetDiagnostics")
            .WithSummary("Gets system diagnostics and health status")
            .Produces<Shared.DTOs.DiagnosticsDto>(200);
    }

    private static async Task<IResult> GetDiagnostics(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDiagnosticsQuery(), cancellationToken);
        return Results.Ok(result);
    }
}
