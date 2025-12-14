using MediatR;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Diagnostics;

/// <summary>
/// Query to retrieve system diagnostics and health status.
/// </summary>
public sealed record GetDiagnosticsQuery : IRequest<DiagnosticsDto>;
