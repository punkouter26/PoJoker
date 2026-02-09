using Azure;
using Azure.Data.Tables;
using Po.Joker.DTOs;
using Po.Joker.Enums;

namespace Po.Joker.Infrastructure.Storage;

/// <summary>
/// Client for interacting with joke performance Table Storage.
/// </summary>
public interface IJokeStorageClient
{
    /// <summary>
    /// Saves a joke performance to Table Storage.
    /// </summary>
    Task SavePerformanceAsync(JokePerformanceDto performance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all performances for a session.
    /// </summary>
    Task<IReadOnlyList<JokePerformanceDto>> GetSessionPerformancesAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session statistics.
    /// </summary>
    Task<SessionStatsDto?> GetSessionStatsAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the leaderboard (top sessions by score).
    /// </summary>
    Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(int top = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of joke storage client using Azure Table Storage.
/// </summary>
public sealed class JokeStorageClient : IJokeStorageClient
{
    private readonly TableClient _tableClient;
    private readonly ILogger<JokeStorageClient> _logger;

    public JokeStorageClient(TableClient tableClient, ILogger<JokeStorageClient> logger)
    {
        _tableClient = tableClient;
        _logger = logger;
    }

    public async Task SavePerformanceAsync(JokePerformanceDto performance, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(performance);

        try
        {
            await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
            _logger.LogDebug("Saved performance {PerformanceId} for session {SessionId}",
                performance.Id, performance.SessionId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to save performance {PerformanceId}", performance.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<JokePerformanceDto>> GetSessionPerformancesAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var performances = new List<JokePerformanceDto>();

        try
        {
            await foreach (var entity in _tableClient.QueryAsync<JokePerformanceEntity>(
                filter: $"PartitionKey eq '{sessionId}'",
                cancellationToken: cancellationToken))
            {
                performances.Add(MapToDto(entity));
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to get performances for session {SessionId}", sessionId);
            throw;
        }

        return performances;
    }

    public async Task<SessionStatsDto?> GetSessionStatsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var performances = await GetSessionPerformancesAsync(sessionId, cancellationToken);

        if (performances.Count == 0)
        {
            return null;
        }

        var triumphs = performances.Count(p => p.IsTriumph);
        var firstPerformance = performances.MinBy(p => p.StartedAt);

        return new SessionStatsDto
        {
            SessionId = sessionId,
            TotalJokes = performances.Count,
            Triumphs = triumphs,
            AverageConfidence = performances.Average(p => p.Analysis.Confidence),
            AverageSimilarity = performances.Average(p => p.Analysis.SimilarityScore),
            AverageLatencyMs = performances.Average(p => p.Analysis.LatencyMs),
            StartedAt = firstPerformance?.StartedAt ?? DateTimeOffset.UtcNow,
            IsRunning = false
        };
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        var sessionStats = new Dictionary<string, (int Total, int Triumphs, DateTimeOffset LastCompleted)>();

        try
        {
            await foreach (var entity in _tableClient.QueryAsync<JokePerformanceEntity>(
                cancellationToken: cancellationToken))
            {
                if (!sessionStats.TryGetValue(entity.SessionId, out var stats))
                {
                    stats = (0, 0, DateTimeOffset.MinValue);
                }

                sessionStats[entity.SessionId] = (
                    stats.Total + 1,
                    stats.Triumphs + (entity.IsTriumph ? 1 : 0),
                    entity.CompletedAt > stats.LastCompleted ? entity.CompletedAt : stats.LastCompleted
                );
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to get leaderboard");
            throw;
        }

        var leaderboard = sessionStats
            .Select(kvp =>
            {
                var triumphRate = kvp.Value.Total > 0
                    ? Math.Round((double)kvp.Value.Triumphs / kvp.Value.Total * 100, 1)
                    : 0;

                return new LeaderboardEntryDto
                {
                    SessionId = kvp.Key,
                    TotalJokes = kvp.Value.Total,
                    Triumphs = kvp.Value.Triumphs,
                    TriumphRate = triumphRate,
                    Score = (kvp.Value.Triumphs * 100) + (triumphRate * 10),
                    CompletedAt = kvp.Value.LastCompleted
                };
            })
            .OrderByDescending(e => e.Score)
            .Take(top)
            .Select((e, i) => e with { Rank = i + 1 })
            .ToList();

        return leaderboard;
    }

    private static JokePerformanceEntity MapToEntity(JokePerformanceDto dto)
    {
        var rowKey = StorageConfiguration.GenerateRowKey(dto.CompletedAt, dto.Id);

        return new JokePerformanceEntity
        {
            PartitionKey = dto.SessionId,
            RowKey = rowKey,
            PerformanceId = dto.Id.ToString(),
            SessionId = dto.SessionId,
            SequenceNumber = dto.SequenceNumber,
            JokeId = dto.Joke.Id,
            JokeCategory = dto.Joke.Category,
            JokeType = dto.Joke.Type,
            JokeSetup = dto.Joke.Setup,
            JokePunchline = dto.Joke.Punchline,
            JokeText = dto.Joke.Joke,
            SafeMode = dto.Joke.SafeMode,
            AiPunchline = dto.Analysis.AiPunchline,
            Confidence = dto.Analysis.Confidence,
            IsTriumph = dto.Analysis.IsTriumph,
            SimilarityScore = dto.Analysis.SimilarityScore,
            AiLatencyMs = dto.Analysis.LatencyMs,
            StartedAt = dto.StartedAt,
            CompletedAt = dto.CompletedAt,
            DurationMs = dto.DurationMs,
            FlagNsfw = dto.Joke.Flags.Nsfw,
            FlagReligious = dto.Joke.Flags.Religious,
            FlagPolitical = dto.Joke.Flags.Political,
            FlagRacist = dto.Joke.Flags.Racist,
            FlagSexist = dto.Joke.Flags.Sexist,
            FlagExplicit = dto.Joke.Flags.Explicit,
            // Rating data (US3)
            RatingCleverness = dto.Analysis.Rating?.Cleverness ?? 0,
            RatingRudeness = dto.Analysis.Rating?.Rudeness ?? 0,
            RatingComplexity = dto.Analysis.Rating?.Complexity ?? 0,
            RatingDifficulty = dto.Analysis.Rating?.Difficulty ?? 0,
            RatingAverage = dto.Analysis.Rating?.Average ?? 0.0,
            RatingCommentary = dto.Analysis.Rating?.Commentary ?? string.Empty
        };
    }

    private static JokePerformanceDto MapToDto(JokePerformanceEntity entity)
    {
        var joke = new JokeDto
        {
            Id = entity.JokeId,
            Category = entity.JokeCategory,
            Type = entity.JokeType,
            Setup = entity.JokeSetup,
            Punchline = entity.JokePunchline,
            Joke = entity.JokeText,
            SafeMode = entity.SafeMode,
            Flags = new JokeFlags
            {
                Nsfw = entity.FlagNsfw,
                Religious = entity.FlagReligious,
                Political = entity.FlagPolitical,
                Racist = entity.FlagRacist,
                Sexist = entity.FlagSexist,
                Explicit = entity.FlagExplicit
            }
        };

        // Restore rating if it was saved
        JokeRatingDto? rating = null;
        if (entity.RatingCleverness > 0 || entity.RatingRudeness > 0 || entity.RatingComplexity > 0 || entity.RatingDifficulty > 0)
        {
            rating = new JokeRatingDto
            {
                Cleverness = entity.RatingCleverness,
                Rudeness = entity.RatingRudeness,
                Complexity = entity.RatingComplexity,
                Difficulty = entity.RatingDifficulty,
                Commentary = entity.RatingCommentary
            };
        }

        var analysis = new JokeAnalysisDto
        {
            Id = Guid.TryParse(entity.PerformanceId, out var id) ? id : Guid.NewGuid(),
            OriginalJoke = joke,
            AiPunchline = entity.AiPunchline,
            Confidence = entity.Confidence,
            IsTriumph = entity.IsTriumph,
            SimilarityScore = entity.SimilarityScore,
            LatencyMs = entity.AiLatencyMs,
            AnalyzedAt = entity.CompletedAt,
            Rating = rating
        };

        return new JokePerformanceDto
        {
            Id = Guid.TryParse(entity.PerformanceId, out var perfId) ? perfId : Guid.NewGuid(),
            SessionId = entity.SessionId,
            Joke = joke,
            Analysis = analysis,
            SequenceNumber = entity.SequenceNumber,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            State = PerformanceState.Transitioning
        };
    }
}
