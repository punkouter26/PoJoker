using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.Features.Diagnostics;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// Integration tests for GET /diag endpoint.
/// Verifies that configuration values are returned with masked secrets.
/// </summary>
public class DiagConfigEndpointTests : IClassFixture<PoJokerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiagConfigEndpointTests(PoJokerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDiag_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/diag");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDiag_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/diag");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetDiag_ReturnsDiagConfigDto()
    {
        // Act
        var response = await _client.GetAsync("/api/diag");
        var diag = await response.Content.ReadFromJsonAsync<DiagConfigDto>();

        // Assert
        diag.Should().NotBeNull();
        diag!.Environment.Should().Be("Development");
        diag.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        diag.Entries.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDiag_ContainsConnectionStringEntries()
    {
        // Act
        var response = await _client.GetAsync("/api/diag");
        var diag = await response.Content.ReadFromJsonAsync<DiagConfigDto>();

        // Assert — the integration test factory injects a "tables" connection string
        diag.Should().NotBeNull();
        diag!.Entries.Should().Contain(e => e.Key.Contains("ConnectionStrings", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDiag_MasksValues()
    {
        // Act
        var response = await _client.GetAsync("/api/diag");
        var diag = await response.Content.ReadFromJsonAsync<DiagConfigDto>();

        // Assert — all non-empty values should be masked (contain asterisks or be "(empty)")
        diag.Should().NotBeNull();
        foreach (var entry in diag!.Entries.Where(e => e.Value != "(empty)"))
        {
            entry.Value.Should().Contain("*",
                $"Value for key '{entry.Key}' should be masked");
        }
    }
}
