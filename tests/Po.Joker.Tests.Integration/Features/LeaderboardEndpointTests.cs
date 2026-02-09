using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// Integration tests for GET /api/leaderboard endpoint.
/// Consolidated with Theory to reduce test count while maintaining coverage.
/// </summary>
public class LeaderboardEndpointTests : IClassFixture<PoJokerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LeaderboardEndpointTests(PoJokerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("Triumph")]
    [InlineData("Cleverness")]
    [InlineData("Rudeness")]
    [InlineData("Complexity")]
    [InlineData("Difficulty")]
    public async Task GetLeaderboard_AllSortOptions_ReturnsOkWithJsonContent(string sortBy)
    {
        // Act
        var response = await _client.GetAsync($"/api/leaderboard?sortBy={sortBy}&top=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsValidEntries_WithSequentialRanks()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=10");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().BeLessThanOrEqualTo(10);
        foreach (var entry in leaderboard)
        {
            entry.SessionId.Should().NotBeNullOrEmpty();
            entry.Rank.Should().BeGreaterThan(0);
        }

        // Ranks should be sequential
        if (leaderboard.Count > 0)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank.Should().Be(i + 1);
            }
        }
    }

    [Theory]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetLeaderboard_WithTopN_ReturnsUpToNEntries(int top)
    {
        // Act
        var response = await _client.GetAsync($"/api/leaderboard?sortBy=Triumph&top={top}");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().BeLessThanOrEqualTo(top);
    }
}
