using MediatR;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Contracts;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Analysis;

/// <summary>
/// MediatR handler for AI joke analysis including rating.
/// Saves performance to storage for leaderboard tracking.
/// </summary>
public sealed class AnalyzeJokeHandler : IRequestHandler<AnalyzeJokeCommand, JokeAnalysisDto>
{
    private readonly IAnalysisService _analysisService;
    private readonly IJokeStorageClient _storageClient;
    private readonly ILogger<AnalyzeJokeHandler> _logger;

    public AnalyzeJokeHandler(
        IAnalysisService analysisService,
        IJokeStorageClient storageClient,
        ILogger<AnalyzeJokeHandler> logger)
    {
        _analysisService = analysisService;
        _storageClient = storageClient;
        _logger = logger;
    }

    public async Task<JokeAnalysisDto> Handle(AnalyzeJokeCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;
        _logger.LogInformation("Analyzing joke Id={JokeId} for session {SessionId}", request.Joke.Id, request.SessionId);

        // Get both analysis and rating in one call
        var (analysis, rating) = await _analysisService.AnalyzeJokeAsync(request.Joke, cancellationToken);

        // Create result with rating included
        var result = analysis with { Rating = rating };

        // Save performance to storage for leaderboard tracking
        var performance = new JokePerformanceDto
        {
            SessionId = request.SessionId,
            Joke = request.Joke,
            Analysis = result,
            StartedAt = startTime,
            CompletedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await _storageClient.SavePerformanceAsync(performance, cancellationToken);
            _logger.LogDebug("Saved performance {PerformanceId} for leaderboard", performance.Id);
        }
        catch (Exception ex)
        {
            // Log but don't fail the analysis if storage fails
            _logger.LogWarning(ex, "Failed to save performance to storage, leaderboard may be incomplete");
        }

        _logger.LogInformation(
            "Analysis complete: IsTriumph={IsTriumph}, Confidence={Confidence:P1}, Latency={LatencyMs}ms, AvgRating={AvgRating:F1}",
            result.IsTriumph, result.Confidence, result.LatencyMs, rating.Average);

        return result;
    }
}
