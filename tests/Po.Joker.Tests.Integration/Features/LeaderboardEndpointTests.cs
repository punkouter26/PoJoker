using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// Integration tests for GET /api/leaderboard endpoint.
/// Tests the full HTTP pipeline for leaderboard operations.
/// </summary>
public class LeaderboardEndpointTests : IClassFixture<PoJokerWebApplicationFactory>
{
    private readonly PoJokerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaderboardEndpointTests(PoJokerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=10");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetLeaderboard_WithDefaultParameters_ReturnsTop10()
    {
        // Act - no parameters should use defaults
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public async Task GetLeaderboard_SortByTriumph_ReturnsValidEntries()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=10");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        foreach (var entry in leaderboard!)
        {
            entry.Should().NotBeNull();
            entry.SessionId.Should().NotBeNullOrEmpty();
            entry.Rank.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetLeaderboard_SortByCleverness_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Cleverness&top=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLeaderboard_SortByRudeness_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Rudeness&top=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLeaderboard_SortByComplexity_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Complexity&top=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLeaderboard_SortByDifficulty_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Difficulty&top=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLeaderboard_WithTop25_ReturnsUpTo25Entries()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=25");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().BeLessThanOrEqualTo(25);
    }

    [Fact]
    public async Task GetLeaderboard_WithTop50_ReturnsUpTo50Entries()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=50");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().BeLessThanOrEqualTo(50);
    }

    [Fact]
    public async Task GetLeaderboard_WithTop100_ReturnsUpTo100Entries()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=100");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard!.Count.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task GetLeaderboard_EntriesHaveSequentialRanks()
    {
        // Act
        var response = await _client.GetAsync("/api/leaderboard?sortBy=Triumph&top=10");
        var leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();

        // Assert
        leaderboard.Should().NotBeNull();
        if (leaderboard!.Count > 0)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank.Should().Be(i + 1, "ranks should be sequential starting from 1");
            }
        }
    }
}
