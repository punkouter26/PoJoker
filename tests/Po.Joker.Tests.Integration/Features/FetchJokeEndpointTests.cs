using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.DTOs;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// TDD integration tests for GET /api/jokes/fetch endpoint.
/// These tests use WebApplicationFactory to test the full HTTP pipeline.
/// </summary>
public class FetchJokeEndpointTests : IClassFixture<PoJokerWebApplicationFactory>
{
    private readonly PoJokerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FetchJokeEndpointTests(PoJokerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetFetch_ReturnsOk_WhenJokeAvailable()
    {
        // Act
        var response = await _client.GetAsync("/api/jokes/fetch?safeMode=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var joke = await response.Content.ReadFromJsonAsync<JokeDto>();
        joke.Should().NotBeNull();
        joke!.Id.Should().BePositive();
        joke.Setup.Should().NotBeEmpty();
        joke.Punchline.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetFetch_WithSafeMode_ReturnsCleanJokes()
    {
        // Act
        var response = await _client.GetAsync("/api/jokes/fetch?safeMode=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var joke = await response.Content.ReadFromJsonAsync<JokeDto>();
        joke!.SafeMode.Should().BeTrue();
    }

    [Fact]
    public async Task GetFetch_WithExcludeIds_ReturnsOk()
    {
        // Act - verify endpoint handles excludeIds parameter gracefully
        // Note: ASP.NET Core expects array parameters in format: excludeIds=1&excludeIds=2&excludeIds=3
        var response = await _client.GetAsync("/api/jokes/fetch?safeMode=true&excludeIds=1&excludeIds=2&excludeIds=3");

        // Assert - should return OK (exclusion is best-effort at application level)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var joke = await response.Content.ReadFromJsonAsync<JokeDto>();
        joke.Should().NotBeNull();
        joke!.Id.Should().BePositive();
    }

    [Fact]
    public async Task GetFetch_ReturnsContentTypeJson()
    {
        // Act
        var response = await _client.GetAsync("/api/jokes/fetch?safeMode=true");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetFetch_ReturnsTwoPartJoke()
    {
        // Act
        var response = await _client.GetAsync("/api/jokes/fetch?safeMode=true");
        var joke = await response.Content.ReadFromJsonAsync<JokeDto>();

        // Assert
        joke!.Type.Should().Be("twopart");
    }
}
