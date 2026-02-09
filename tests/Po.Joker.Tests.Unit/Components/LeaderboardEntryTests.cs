using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Po.Joker.Components;
using Po.Joker.DTOs;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// TDD tests for LeaderboardEntry.razor component.
/// Tests display of individual leaderboard entries with ratings.
/// </summary>
public class LeaderboardEntryTests : BunitContext
{
    [Fact]
    public void LeaderboardEntry_ShouldDisplayRank()
    {
        // Arrange
        var entry = new LeaderboardEntryDto
        {
            Rank = 1,
            SessionId = "session-123",
            TotalJokes = 25,
            Triumphs = 15,
            TriumphRate = 60.0,
            Score = 2100
        };

        // Act
        var cut = Render<LeaderboardEntry>(parameters => parameters
            .Add(p => p.Entry, entry));

        // Assert
        cut.Markup.Should().Contain("1");
        cut.Find(".leaderboard-entry").Should().NotBeNull();
    }

    [Fact]
    public void LeaderboardEntry_ShouldDisplayTriumphs()
    {
        // Arrange
        var entry = new LeaderboardEntryDto
        {
            Rank = 2,
            SessionId = "session-456",
            TotalJokes = 30,
            Triumphs = 20,
            TriumphRate = 66.7,
            Score = 2667
        };

        // Act
        var cut = Render<LeaderboardEntry>(parameters => parameters
            .Add(p => p.Entry, entry));

        // Assert
        cut.Markup.Should().Contain("20");
        cut.Markup.Should().Contain("30");
    }

    [Fact]
    public void LeaderboardEntry_ShouldDisplayTriumphRate()
    {
        // Arrange
        var entry = new LeaderboardEntryDto
        {
            Rank = 3,
            SessionId = "session-789",
            TotalJokes = 10,
            Triumphs = 7,
            TriumphRate = 70.0,
            Score = 1400
        };

        // Act
        var cut = Render<LeaderboardEntry>(parameters => parameters
            .Add(p => p.Entry, entry));

        // Assert
        cut.Markup.Should().Contain("70");
    }

    [Fact]
    public void LeaderboardEntry_ShouldHighlightCurrentSession()
    {
        // Arrange
        var entry = new LeaderboardEntryDto
        {
            Rank = 1,
            SessionId = "current-session",
            TotalJokes = 50,
            Triumphs = 40,
            TriumphRate = 80.0,
            Score = 4800,
            IsCurrentSession = true
        };

        // Act
        var cut = Render<LeaderboardEntry>(parameters => parameters
            .Add(p => p.Entry, entry));

        // Assert
        cut.Find(".leaderboard-entry").ClassList.Should().Contain("current-session");
    }

    [Fact]
    public void LeaderboardEntry_ShouldDisplayScore()
    {
        // Arrange
        var entry = new LeaderboardEntryDto
        {
            Rank = 5,
            SessionId = "session-abc",
            TotalJokes = 15,
            Triumphs = 8,
            TriumphRate = 53.3,
            Score = 1333
        };

        // Act
        var cut = Render<LeaderboardEntry>(parameters => parameters
            .Add(p => p.Entry, entry));

        // Assert
        cut.Markup.Should().Contain("1333");
    }

    [Fact]
    public void LeaderboardEntry_ShouldShowMedalForTopThree()
    {
        // Arrange - Gold medal for rank 1
        var entry = new LeaderboardEntryDto
        {
            Rank = 1,
            SessionId = "champion",
            TotalJokes = 100,
            Triumphs = 90,
            TriumphRate = 90.0,
            Score = 9900
        };

        // Act
        var cut = Render<LeaderboardEntry>(parameters => parameters
            .Add(p => p.Entry, entry));

        // Assert - Should have medal indicator for top 3
        var markup = cut.Markup;
        (markup.Contains("🥇") || markup.Contains("🥈") || markup.Contains("🥉") || markup.Contains("medal")).Should().BeTrue();
    }
}
