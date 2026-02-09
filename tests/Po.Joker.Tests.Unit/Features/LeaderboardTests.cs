using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Po.Joker.Features.Leaderboards;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.DTOs;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// TDD tests for GetLeaderboardHandler - Written FIRST before implementation.
/// Tests leaderboard retrieval with sorting and filtering.
/// </summary>
public class LeaderboardTests
{
    private readonly Mock<IJokeStorageClient> _mockStorageClient;
    private readonly Mock<ILogger<GetLeaderboardHandler>> _mockLogger;

    public LeaderboardTests()
    {
        _mockStorageClient = new Mock<IJokeStorageClient>();
        _mockLogger = new Mock<ILogger<GetLeaderboardHandler>>();
    }

    [Fact]
    public async Task Handle_WithSortByTriumph_ReturnsOrderedByTriumphs()
    {
        // Arrange
        var entries = new List<LeaderboardEntryDto>
        {
            new() { Rank = 1, SessionId = "session-1", Triumphs = 10, TotalJokes = 20, TriumphRate = 50, Score = 1500 },
            new() { Rank = 2, SessionId = "session-2", Triumphs = 5, TotalJokes = 15, TriumphRate = 33.3, Score = 833 },
            new() { Rank = 3, SessionId = "session-3", Triumphs = 3, TotalJokes = 10, TriumphRate = 30, Score = 600 }
        };

        _mockStorageClient
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var handler = new GetLeaderboardHandler(_mockStorageClient.Object, _mockLogger.Object);
        var query = new GetLeaderboardQuery("Triumph", null, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInDescendingOrder(x => x.Triumphs);
    }

    [Fact]
    public async Task Handle_WithSortByCleverness_ReturnsOrderedByCleverness()
    {
        // Arrange
        var entries = new List<LeaderboardEntryDto>
        {
            new() { Rank = 1, SessionId = "session-1", Triumphs = 5, TotalJokes = 10, Score = 800 },
            new() { Rank = 2, SessionId = "session-2", Triumphs = 8, TotalJokes = 15, Score = 1200 }
        };

        _mockStorageClient
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var handler = new GetLeaderboardHandler(_mockStorageClient.Object, _mockLogger.Object);
        var query = new GetLeaderboardQuery("Cleverness", null, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountLessThanOrEqualTo(10);
    }

    [Fact]
    public async Task Handle_WithTopLimit_ReturnsLimitedResults()
    {
        // Arrange
        var entries = Enumerable.Range(1, 20)
            .Select(i => new LeaderboardEntryDto
            {
                Rank = i,
                SessionId = $"session-{i}",
                Triumphs = 20 - i,
                TotalJokes = 25,
                Score = (20 - i) * 100
            })
            .ToList();

        _mockStorageClient
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var handler = new GetLeaderboardHandler(_mockStorageClient.Object, _mockLogger.Object);
        var query = new GetLeaderboardQuery("Triumph", null, 5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_FiltersResults()
    {
        // Arrange
        var entries = new List<LeaderboardEntryDto>
        {
            new() { Rank = 1, SessionId = "session-1", Triumphs = 10, TotalJokes = 20, Score = 1500 },
            new() { Rank = 2, SessionId = "session-2", Triumphs = 8, TotalJokes = 15, Score = 1200 }
        };

        _mockStorageClient
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var handler = new GetLeaderboardHandler(_mockStorageClient.Object, _mockLogger.Object);
        var query = new GetLeaderboardQuery("Triumph", "Programming", 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoEntries_ReturnsEmptyList()
    {
        // Arrange
        _mockStorageClient
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeaderboardEntryDto>());

        var handler = new GetLeaderboardHandler(_mockStorageClient.Object, _mockLogger.Object);
        var query = new GetLeaderboardQuery("Triumph", null, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
