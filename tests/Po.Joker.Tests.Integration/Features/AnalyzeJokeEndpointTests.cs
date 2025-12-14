using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.Shared.DTOs;
using Po.Joker.Shared.Contracts;
using Po.Joker.Features.Analysis;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// TDD integration tests for POST /api/jokes/analyze endpoint.
/// These tests verify AI analysis of jokes through the full pipeline.
/// Uses MockAnalysisService to avoid calling real Azure OpenAI (no cost).
/// </summary>
public class AnalyzeJokeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AnalyzeJokeEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real AI service with mock to avoid Azure OpenAI costs
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAnalysisService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton<IAnalysisService, MockAnalysisService>();
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostAnalyze_ReturnsOk_WhenJokeValid()
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/jokes/analyze", joke);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var analysis = await response.Content.ReadFromJsonAsync<JokeAnalysisDto>();
        analysis.Should().NotBeNull();
    }

    [Fact]
    public async Task PostAnalyze_ReturnsAiPunchline()
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/jokes/analyze", joke);
        var analysis = await response.Content.ReadFromJsonAsync<JokeAnalysisDto>();

        // Assert
        analysis!.AiPunchline.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostAnalyze_ReturnsConfidenceScore()
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/jokes/analyze", joke);
        var analysis = await response.Content.ReadFromJsonAsync<JokeAnalysisDto>();

        // Assert
        analysis!.Confidence.Should().BeInRange(0, 1);
    }

    [Fact]
    public async Task PostAnalyze_ReturnsLatencyMs()
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/jokes/analyze", joke);
        var analysis = await response.Content.ReadFromJsonAsync<JokeAnalysisDto>();

        // Assert
        analysis!.LatencyMs.Should().BePositive();
    }

    [Fact]
    public async Task PostAnalyze_ReturnsBadRequest_WhenJokeInvalid()
    {
        // Arrange - joke with empty setup/punchline (validation should fail)
        var invalidJoke = new JokeDto
        {
            Id = 1,
            Category = "Test",
            Type = "invalid", // Invalid type
            Setup = "", // Empty setup
            Punchline = "" // Empty punchline
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/jokes/analyze", invalidJoke);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAnalyze_ReturnsSimilarityScore()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 111,
            Category = "Programming",
            Type = "twopart",
            Setup = "Test",
            Punchline = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/jokes/analyze", joke);
        var analysis = await response.Content.ReadFromJsonAsync<JokeAnalysisDto>();

        // Assert
        analysis!.SimilarityScore.Should().BeInRange(0, 1);
    }
}
