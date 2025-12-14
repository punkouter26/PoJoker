using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Unit.Pages;

/// <summary>
/// Unit tests for Diag.razor page.
/// </summary>
public sealed class DiagPageTests : BunitContext
{
    // Note: These tests verify the DiagnosticsDto structure and basic rendering concepts.
    // Full page tests require WebApplicationFactory integration testing.

    [Fact]
    public void DiagnosticsDto_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var dto = new DiagnosticsDto
        {
            Environment = "Development"
        };

        // Assert
        dto.Version.Should().Be("1.0.0");
        dto.Status.Should().Be(HealthStatus.Healthy);
        dto.Services.Should().BeEmpty();
    }

    [Fact]
    public void ServiceHealthDto_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var dto = new ServiceHealthDto
        {
            Name = "TestService"
        };

        // Assert
        dto.Status.Should().Be(HealthStatus.Healthy);
        dto.ResponseTimeMs.Should().Be(0);
        dto.Message.Should().BeNull();
    }

    [Fact]
    public void HealthStatus_HasCorrectEnumValues()
    {
        // Assert
        Enum.GetValues<HealthStatus>().Should().HaveCount(3);
        ((int)HealthStatus.Healthy).Should().Be(0);
        ((int)HealthStatus.Degraded).Should().Be(1);
        ((int)HealthStatus.Unhealthy).Should().Be(2);
    }

    [Fact]
    public void DiagnosticsDto_CanHaveMultipleServices()
    {
        // Arrange & Act
        var dto = new DiagnosticsDto
        {
            Environment = "Production",
            Services =
            [
                new ServiceHealthDto { Name = "JokeAPI", Status = HealthStatus.Healthy },
                new ServiceHealthDto { Name = "AzureOpenAI", Status = HealthStatus.Degraded },
                new ServiceHealthDto { Name = "TableStorage", Status = HealthStatus.Unhealthy }
            ]
        };

        // Assert
        dto.Services.Should().HaveCount(3);
        dto.Services[0].Name.Should().Be("JokeAPI");
        dto.Services[1].Status.Should().Be(HealthStatus.Degraded);
        dto.Services[2].Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public void DiagnosticsDto_TracksMetrics()
    {
        // Arrange & Act
        var dto = new DiagnosticsDto
        {
            Environment = "Test",
            TotalJokesServed = 100,
            TotalAnalyses = 50,
            TriumphRate = 0.75
        };

        // Assert
        dto.TotalJokesServed.Should().Be(100);
        dto.TotalAnalyses.Should().Be(50);
        dto.TriumphRate.Should().Be(0.75);
    }

    [Fact]
    public void DiagnosticsDto_TracksUptime()
    {
        // Arrange
        var uptime = TimeSpan.FromHours(2.5);

        // Act
        var dto = new DiagnosticsDto
        {
            Environment = "Test",
            Uptime = uptime
        };

        // Assert
        dto.Uptime.Should().Be(uptime);
        dto.Uptime.TotalHours.Should().BeApproximately(2.5, 0.01);
    }
}
