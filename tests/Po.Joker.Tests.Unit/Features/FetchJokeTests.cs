using Po.Joker.Features.Jokes;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// TDD tests for FetchJokeHandler - Written FIRST before implementation.
/// These tests verify joke fetching from JokeAPI with safe mode filtering.
/// </summary>
public class FetchJokeTests
{
    private readonly Mock<IJokeApiClient> _mockJokeApiClient;
    private readonly Mock<ILogger<FetchJokeHandler>> _mockLogger;

    public FetchJokeTests()
    {
        _mockJokeApiClient = new Mock<IJokeApiClient>();
        _mockLogger = new Mock<ILogger<FetchJokeHandler>>();
    }

    [Fact]
    public async Task Handle_WithSafeMode_ReturnsTwoPartJoke()
    {
        // Arrange
        var expectedJoke = new JokeDto
        {
            Id = 123,
            Category = "Programming",
            Type = "twopart",
            Setup = "Why do programmers prefer dark mode?",
            Punchline = "Because light attracts bugs!",
            SafeMode = true
        };

        _mockJokeApiClient
            .Setup(x => x.FetchJokeAsync(true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJoke);

        var handler = new FetchJokeHandler(_mockJokeApiClient.Object, _mockLogger.Object);
        var query = new FetchJokeQuery(SafeMode: true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(123);
        result.Type.Should().Be("twopart");
        result.Setup.Should().NotBeEmpty();
        result.Punchline.Should().NotBeEmpty();
        result.SafeMode.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithSafeModeDisabled_AllowsAllContent()
    {
        // Arrange
        var expectedJoke = new JokeDto
        {
            Id = 456,
            Category = "Dark",
            Type = "twopart",
            Setup = "Some dark setup",
            Punchline = "Some dark punchline",
            SafeMode = false,
            Flags = new JokeFlags { Nsfw = true }
        };

        _mockJokeApiClient
            .Setup(x => x.FetchJokeAsync(false, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJoke);

        var handler = new FetchJokeHandler(_mockJokeApiClient.Object, _mockLogger.Object);
        var query = new FetchJokeQuery(SafeMode: false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SafeMode.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExcludesDuplicateJokeIds()
    {
        // Arrange
        var excludeIds = new[] { 100, 200, 300 };
        var expectedJoke = new JokeDto
        {
            Id = 999,
            Category = "Pun",
            Type = "twopart",
            Setup = "Fresh joke setup",
            Punchline = "Fresh punchline"
        };

        _mockJokeApiClient
            .Setup(x => x.FetchJokeAsync(true, excludeIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJoke);

        var handler = new FetchJokeHandler(_mockJokeApiClient.Object, _mockLogger.Object);
        var query = new FetchJokeQuery(SafeMode: true, ExcludeIds: excludeIds);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        excludeIds.Should().NotContain(result.Id);
        _mockJokeApiClient.Verify(
            x => x.FetchJokeAsync(true, excludeIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiThrows_PropagatesException()
    {
        // Arrange
        _mockJokeApiClient
            .Setup(x => x.FetchJokeAsync(It.IsAny<bool>(), It.IsAny<IEnumerable<int>?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        var handler = new FetchJokeHandler(_mockJokeApiClient.Object, _mockLogger.Object);
        var query = new FetchJokeQuery(SafeMode: true);

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*API unavailable*");
    }
}
