using MediatR;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Analysis;

/// <summary>
/// MediatR command to analyze a joke using AI punchline prediction.
/// </summary>
/// <param name="Joke">The joke to analyze (setup will be sent to AI).</param>
/// <param name="SessionId">Session identifier for leaderboard tracking.</param>
public sealed record AnalyzeJokeCommand(JokeDto Joke, string SessionId) : IRequest<JokeAnalysisDto>;
