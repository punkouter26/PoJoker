using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// Integration tests for GET /api/diagnostics endpoint.
/// Consolidated with Theory to reduce test count while maintaining coverage.
/// </summary>
public class DiagnosticsEndpointTests : IClassFixture<PoJokerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiagnosticsEndpointTests(PoJokerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsJsonWithExpectedShape()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();
        diagnostics.Should().NotBeNull();
        diagnostics!.Version.Should().NotBeNullOrEmpty();
        diagnostics.Environment.Should().NotBeNullOrEmpty();
        diagnostics.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsStatusAndUptime()
    {
        // Act
        var diagnostics = await GetDiagnosticsAsync();

        // Assert
        diagnostics.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
        diagnostics.Uptime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        diagnostics.TotalJokesServed.Should().BeGreaterThanOrEqualTo(0);
        diagnostics.TotalAnalyses.Should().BeGreaterThanOrEqualTo(0);
        diagnostics.TriumphRate.Should().BeInRange(0.0, 100.0);
    }

    [Theory]
    [InlineData("JokeAPI")]
    [InlineData("AzureOpenAI")]
    [InlineData("TableStorage")]
    public async Task GetDiagnostics_IncludesExpectedHealthCheck(string serviceName)
    {
        // Act
        var diagnostics = await GetDiagnosticsAsync();

        // Assert
        diagnostics.Services.Should().Contain(s => s.Name == serviceName,
            $"{serviceName} health check should be included");
    }

    [Fact]
    public async Task GetDiagnostics_ServiceHealthHasValidProperties()
    {
        // Act
        var diagnostics = await GetDiagnosticsAsync();

        // Assert
        foreach (var service in diagnostics.Services)
        {
            service.Name.Should().NotBeNullOrEmpty();
            service.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
            service.LastChecked.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));

            if (service.Status == HealthStatus.Healthy)
                service.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);

            if (service.Status == HealthStatus.Unhealthy)
                service.Message.Should().NotBeNullOrEmpty();
        }
    }

    private async Task<DiagnosticsDto> GetDiagnosticsAsync()
    {
        var response = await _client.GetAsync("/api/diagnostics");
        var dto = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();
        dto.Should().NotBeNull();
        return dto!;
    }
}
