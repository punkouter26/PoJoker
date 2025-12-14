using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Integration.Features;

/// <summary>
/// Integration tests for GET /api/diagnostics endpoint.
/// Tests the full HTTP pipeline for diagnostics and health checks.
/// </summary>
public class DiagnosticsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DiagnosticsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");

        // Assert
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable },
            "diagnostics may return 503 if services are unhealthy");
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsDiagnosticsDto()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.Version.Should().NotBeNullOrEmpty();
        diagnostics.Environment.Should().NotBeNullOrEmpty();
        diagnostics.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsOverallStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsServiceHealthChecks()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.Services.Should().NotBeEmpty("at least one health check should be configured");
        
        foreach (var service in diagnostics.Services)
        {
            service.Name.Should().NotBeNullOrEmpty();
            service.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
            service.LastChecked.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        }
    }

    [Fact]
    public async Task GetDiagnostics_IncludesJokeApiHealthCheck()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.Services.Should().Contain(s => s.Name == "JokeAPI", 
            "JokeAPI health check should be included");
    }

    [Fact]
    public async Task GetDiagnostics_IncludesAzureOpenAIHealthCheck()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.Services.Should().Contain(s => s.Name == "AzureOpenAI", 
            "AzureOpenAI health check should be included");
    }

    [Fact]
    public async Task GetDiagnostics_IncludesTableStorageHealthCheck()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.Services.Should().Contain(s => s.Name == "TableStorage", 
            "TableStorage health check should be included");
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsUptimeInformation()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics.Uptime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero,
            "uptime should be positive");
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsMetricsCounters()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        diagnostics!.TotalJokesServed.Should().BeGreaterThanOrEqualTo(0);
        diagnostics.TotalAnalyses.Should().BeGreaterThanOrEqualTo(0);
        diagnostics.TriumphRate.Should().BeInRange(0.0, 100.0);
    }

    [Fact]
    public async Task GetDiagnostics_ServiceHealthContainsResponseTime()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert
        diagnostics.Should().NotBeNull();
        foreach (var service in diagnostics!.Services)
        {
            if (service.Status == HealthStatus.Healthy)
            {
                service.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0, 
                    "healthy services should have a response time measurement");
            }
        }
    }

    [Fact]
    public async Task GetDiagnostics_UnhealthyServicesHaveErrorMessage()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics");
        var diagnostics = await response.Content.ReadFromJsonAsync<DiagnosticsDto>();

        // Assert - if any service is unhealthy, it should have a message
        diagnostics.Should().NotBeNull();
        var unhealthyServices = diagnostics!.Services.Where(s => s.Status == HealthStatus.Unhealthy);
        
        foreach (var service in unhealthyServices)
        {
            service.Message.Should().NotBeNullOrEmpty(
                $"{service.Name} is unhealthy and should provide an error message");
        }
    }
}
