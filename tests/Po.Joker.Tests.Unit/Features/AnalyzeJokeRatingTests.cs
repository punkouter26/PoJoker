using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Po.Joker.Features.Analysis;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Contracts;
using Po.Joker.DTOs;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// TDD tests for joke rating generation in AnalyzeJokeHandler.
/// Tests that the handler properly includes rating data in the response.
/// </summary>
public class AnalyzeJokeRatingTests
{
    private readonly Mock<IAnalysisService> _mockAnalysisService;
    private readonly Mock<IJokeStorageClient> _mockStorageClient;
    private readonly Mock<ILogger<AnalyzeJokeHandler>> _mockLogger;
    private readonly AnalyzeJokeHandler _handler;
    private const string TestSessionId = "test-session";

    public AnalyzeJokeRatingTests()
    {
        _mockAnalysisService = new Mock<IAnalysisService>();
        _mockStorageClient = new Mock<IJokeStorageClient>();
        _mockLogger = new Mock<ILogger<AnalyzeJokeHandler>>();
        _handler = new AnalyzeJokeHandler(_mockAnalysisService.Object, _mockStorageClient.Object, _mockLogger.Object);
    }

    private static JokeDto CreateTestJoke() => new()
    {
        Id = 42,
        Category = "Programming",
        Type = "twopart",
        Setup = "Why do programmers prefer dark mode?",
        Punchline = "Because light attracts bugs!"
    };

    [Fact]
    public async Task Handle_ShouldReturnAnalysisWithRating()
    {
        // Arrange
        var joke = CreateTestJoke();
        var expectedAnalysis = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "Because bugs are attracted to light!",
            Confidence = 0.85,
            IsTriumph = true,
            SimilarityScore = 0.78,
            LatencyMs = 150
        };

        var expectedRating = new JokeRatingDto
        {
            Cleverness = 8,
            Rudeness = 2,
            Complexity = 5,
            Difficulty = 6,
            Commentary = "A classic programmer humor that never gets old!"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(It.IsAny<JokeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedAnalysis, expectedRating));

        var command = new AnalyzeJokeCommand(joke, TestSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().NotBeNull();
        result.Rating!.Cleverness.Should().Be(8);
        result.Rating.Rudeness.Should().Be(2);
        result.Rating.Commentary.Should().Contain("programmer");
    }

    [Fact]
    public async Task Handle_ShouldIncludeAllRatingDimensions()
    {
        // Arrange
        var joke = CreateTestJoke();
        var analysis = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "Test",
            Confidence = 0.5,
            IsTriumph = false,
            SimilarityScore = 0.2,
            LatencyMs = 100
        };

        var rating = new JokeRatingDto
        {
            Cleverness = 7,
            Rudeness = 3,
            Complexity = 6,
            Difficulty = 8,
            Commentary = "This jest requires the wisdom of the ancients!"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(It.IsAny<JokeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((analysis, rating));

        // Act
        var result = await _handler.Handle(new AnalyzeJokeCommand(joke, TestSessionId), CancellationToken.None);

        // Assert
        result.Rating.Should().NotBeNull();
        result.Rating!.Cleverness.Should().BeInRange(1, 10);
        result.Rating.Rudeness.Should().BeInRange(1, 10);
        result.Rating.Complexity.Should().BeInRange(1, 10);
        result.Rating.Difficulty.Should().BeInRange(1, 10);
        result.Rating.Average.Should().Be(6.0); // (7+3+6+8)/4
    }

    [Fact]
    public async Task Handle_ShouldIncludeJesterCommentary()
    {
        // Arrange
        var joke = CreateTestJoke();
        var analysis = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "Test punchline",
            Confidence = 0.7,
            IsTriumph = false,
            SimilarityScore = 0.4,
            LatencyMs = 200
        };

        var rating = new JokeRatingDto
        {
            Cleverness = 5,
            Rudeness = 1,
            Complexity = 3,
            Difficulty = 4,
            Commentary = "Verily, this jest doth tickle mine funny bone most gently!"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(It.IsAny<JokeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((analysis, rating));

        // Act
        var result = await _handler.Handle(new AnalyzeJokeCommand(joke, TestSessionId), CancellationToken.None);

        // Assert
        result.Rating!.Commentary.Should().NotBeNullOrEmpty();
        result.Rating.Commentary.Should().Contain("jest");
    }

    [Fact]
    public async Task Handle_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var joke = CreateTestJoke();
        var analysis = new JokeAnalysisDto
        {
            OriginalJoke = joke,
            AiPunchline = "Guess",
            Confidence = 0.5,
            IsTriumph = false,
            SimilarityScore = 0.1,
            LatencyMs = 50
        };

        var rating = new JokeRatingDto
        {
            Cleverness = 10,
            Rudeness = 2,
            Complexity = 4,
            Difficulty = 8,
            Commentary = "Test"
        };

        _mockAnalysisService
            .Setup(x => x.AnalyzeJokeAsync(It.IsAny<JokeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((analysis, rating));

        // Act
        var result = await _handler.Handle(new AnalyzeJokeCommand(joke, TestSessionId), CancellationToken.None);

        // Assert
        result.Rating!.Average.Should().Be(6.0); // (10+2+4+8)/4 = 24/4 = 6.0
    }
}
