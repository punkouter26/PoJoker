using System.Diagnostics;
using Po.Joker.Shared.Contracts;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Features.Analysis;

/// <summary>
/// Mock AI analysis service for development without Azure OpenAI.
/// Returns random punchline predictions.
/// </summary>
public sealed class MockAnalysisService : IAnalysisService
{
    private static readonly string[] MockPunchlines =
    [
        "Because they can't handle the byte!",
        "It was too mainstream!",
        "They wanted to C# clearly!",
        "Because it had no body!",
        "It couldn't find its class!",
        "They lost their inheritance!",
        "It kept throwing exceptions!",
        "The algorithm was too complex!"
    ];

    private readonly Random _random = new();
    private readonly ILogger<MockAnalysisService> _logger;

    public MockAnalysisService(ILogger<MockAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<JokeAnalysisDto> PredictPunchlineAsync(JokeDto joke, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Simulate AI processing time
        await Task.Delay(_random.Next(200, 800), cancellationToken);

        var mockPunchline = MockPunchlines[_random.Next(MockPunchlines.Length)];

        stopwatch.Stop();

        // Random chance of triumph (20% to make it interesting)
        var isTriumph = _random.NextDouble() < 0.2;
        var similarityScore = isTriumph ? _random.NextDouble() * 0.2 + 0.8 : _random.NextDouble() * 0.5;

        _logger.LogInformation(
            "[MOCK] AI predicted punchline for joke {JokeId}: IsTriumph={IsTriumph}",
            joke.Id, isTriumph);

        return new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = mockPunchline,
            Confidence = _random.NextDouble() * 0.3 + 0.6,
            IsTriumph = isTriumph,
            SimilarityScore = similarityScore,
            LatencyMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<JokeRatingDto> RateJokeAsync(JokeDto joke, CancellationToken cancellationToken = default)
    {
        await Task.Delay(_random.Next(100, 300), cancellationToken);

        return new JokeRatingDto
        {
            Cleverness = _random.Next(1, 11),
            Rudeness = _random.Next(1, 5),
            Complexity = _random.Next(1, 11),
            Difficulty = _random.Next(1, 11),
            Commentary = "A jest of reasonable mirth!"
        };
    }

    public async Task<(JokeAnalysisDto Analysis, JokeRatingDto Rating)> AnalyzeJokeAsync(JokeDto joke, CancellationToken cancellationToken = default)
    {
        var analysis = await PredictPunchlineAsync(joke, cancellationToken);
        var rating = await RateJokeAsync(joke, cancellationToken);
        return (analysis, rating);
    }
}
