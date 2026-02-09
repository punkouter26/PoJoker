using Po.Joker.Features.Analysis;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Contracts;
using Po.Joker.DTOs;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// TDD tests for AnalyzeJokeHandler - Written FIRST before implementation.
/// These tests verify AI punchline prediction and analysis with ratings.
/// </summary>
public class AnalyzeJokeTests
{
    private readonly Mock<IAnalysisService> _mockAnalysisService;
    private readonly Mock<IJokeStorageClient> _mockStorageClient;
    private readonly Mock<ILogger<AnalyzeJokeHandler>> _mockLogger;
    private const string TestSessionId = "test-session-123";

    public AnalyzeJokeTests()
    {
        _mockAnalysisService = new Mock<IAnalysisService>();
        _mockStorageClient = new Mock<IJokeStorageClient>();
        _mockLogger = new Mock<ILogger<AnalyzeJokeHandler>>();
    }

    [Fact]
    public async Task Handle_WithValidJoke_ReturnsAnalysis()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 123,
            Category = "Programming",
            Type = "twopart",
            Setup = "Why do programmers prefer dark mode?",
            Punchline = "Because light attracts bugs!"
        };

        var expectedAnalysis = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "Because they hate bright screens!",
            Confidence = 0.75,
            IsTriumph = false,
            SimilarityScore = 0.3,
            LatencyMs = 250
        };

        var expectedRating = new JokeRatingDto
        {
            Cleverness = 7,
            Rudeness = 2,
            Complexity = 5,
            Difficulty = 6,
            Commentary = "A classic tech joke!"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(joke, It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedAnalysis, expectedRating));

        var handler = new AnalyzeJokeHandler(_mockAnalysisService.Object, _mockStorageClient.Object, _mockLogger.Object);
        var command = new AnalyzeJokeCommand(joke, TestSessionId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OriginalJoke.Should().Be(joke);
        result.AiPunchline.Should().NotBeEmpty();
        result.Confidence.Should().BeInRange(0, 1);
    }

    [Fact]
    public async Task Handle_WhenAiGuessesCorrectly_MarksAsTriumph()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 456,
            Category = "Pun",
            Type = "twopart",
            Setup = "What do you call a fake noodle?",
            Punchline = "An impasta!"
        };

        var triumphAnalysis = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "An impasta!",
            Confidence = 0.95,
            IsTriumph = true,
            SimilarityScore = 1.0,
            LatencyMs = 180
        };

        var triumphRating = new JokeRatingDto
        {
            Cleverness = 9,
            Rudeness = 1,
            Complexity = 3,
            Difficulty = 4,
            Commentary = "The Jester triumphs!"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(joke, It.IsAny<CancellationToken>()))
            .ReturnsAsync((triumphAnalysis, triumphRating));

        var handler = new AnalyzeJokeHandler(_mockAnalysisService.Object, _mockStorageClient.Object, _mockLogger.Object);
        var command = new AnalyzeJokeCommand(joke, TestSessionId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsTriumph.Should().BeTrue();
        result.SimilarityScore.Should().BeGreaterThanOrEqualTo(0.8);
    }

    [Fact]
    public async Task Handle_TracksLatency()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 789,
            Category = "Programming",
            Type = "twopart",
            Setup = "Test setup",
            Punchline = "Test punchline"
        };

        var analysisWithLatency = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "Some guess",
            Confidence = 0.5,
            IsTriumph = false,
            SimilarityScore = 0.2,
            LatencyMs = 500
        };

        var rating = new JokeRatingDto
        {
            Cleverness = 5,
            Rudeness = 5,
            Complexity = 5,
            Difficulty = 5,
            Commentary = "Average jest"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(joke, It.IsAny<CancellationToken>()))
            .ReturnsAsync((analysisWithLatency, rating));

        var handler = new AnalyzeJokeHandler(_mockAnalysisService.Object, _mockStorageClient.Object, _mockLogger.Object);
        var command = new AnalyzeJokeCommand(joke, TestSessionId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.LatencyMs.Should().BePositive();
    }

    [Fact]
    public async Task Handle_WhenAiServiceFails_PropagatesException()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 999,
            Category = "Misc",
            Type = "twopart",
            Setup = "Any setup",
            Punchline = "Any punchline"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(It.IsAny<JokeDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("AI service timed out"));

        var handler = new AnalyzeJokeHandler(_mockAnalysisService.Object, _mockStorageClient.Object, _mockLogger.Object);
        var command = new AnalyzeJokeCommand(joke, TestSessionId);

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<TimeoutException>()
            .WithMessage("*timed out*");
    }
}
