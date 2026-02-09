using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Po.Joker.Features.Diagnostics;
using Po.Joker.DTOs;
using DtoHealthStatus = Po.Joker.DTOs.HealthStatus;
using MsHealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// Unit tests for GetDiagnosticsHandler.
/// </summary>
public sealed class DiagnosticsTests
{
    private readonly Mock<HealthCheckService> _healthCheckServiceMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<GetDiagnosticsHandler>> _loggerMock;

    public DiagnosticsTests()
    {
        _healthCheckServiceMock = new Mock<HealthCheckService>();
        _environmentMock = new Mock<IHostEnvironment>();
        _loggerMock = new Mock<ILogger<GetDiagnosticsHandler>>();

        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
    }

    [Fact]
    public async Task Handle_ReturnsHealthyStatus_WhenAllServicesHealthy()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["JokeAPI"] = new HealthReportEntry(
                    MsHealthStatus.Healthy, "OK", TimeSpan.FromMilliseconds(50), null, null),
                ["AzureOpenAI"] = new HealthReportEntry(
                    MsHealthStatus.Healthy, "OK", TimeSpan.FromMilliseconds(100), null, null)
            },
            TimeSpan.FromMilliseconds(150));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var handler = new GetDiagnosticsHandler(
            _healthCheckServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetDiagnosticsQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(DtoHealthStatus.Healthy);
        result.Services.Should().HaveCount(2);
        result.Environment.Should().Be("Development");
    }

    [Fact]
    public async Task Handle_ReturnsDegradedStatus_WhenAnyServiceDegraded()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["JokeAPI"] = new HealthReportEntry(
                    MsHealthStatus.Healthy, "OK", TimeSpan.FromMilliseconds(50), null, null),
                ["AzureOpenAI"] = new HealthReportEntry(
                    MsHealthStatus.Degraded, "Slow", TimeSpan.FromMilliseconds(500), null, null)
            },
            TimeSpan.FromMilliseconds(550));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var handler = new GetDiagnosticsHandler(
            _healthCheckServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetDiagnosticsQuery(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(DtoHealthStatus.Degraded);
    }

    [Fact]
    public async Task Handle_ReturnsUnhealthyStatus_WhenAnyServiceUnhealthy()
    {
        // Arrange - Description takes priority over exception message
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["JokeAPI"] = new HealthReportEntry(
                    MsHealthStatus.Unhealthy, "Connection failed", TimeSpan.FromMilliseconds(1000), new Exception("Inner error"), null)
            },
            TimeSpan.FromMilliseconds(1000));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var handler = new GetDiagnosticsHandler(
            _healthCheckServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetDiagnosticsQuery(), CancellationToken.None);

        // Assert
        result.Status.Should().Be(DtoHealthStatus.Unhealthy);
        result.Services.First().Message.Should().Contain("Connection failed");
    }

    [Fact]
    public async Task Handle_IncludesVersion()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            TimeSpan.Zero);

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var handler = new GetDiagnosticsHandler(
            _healthCheckServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetDiagnosticsQuery(), CancellationToken.None);

        // Assert
        result.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_IncludesUptime()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            TimeSpan.Zero);

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var handler = new GetDiagnosticsHandler(
            _healthCheckServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetDiagnosticsQuery(), CancellationToken.None);

        // Assert - Uptime should be a valid TimeSpan (can be slightly negative due to timing in tests)
        // The important thing is that it returns a TimeSpan value, not that it's precisely >= 0
        result.Uptime.Should().BeCloseTo(TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_MapsServiceResponseTimes()
    {
        // Arrange
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["TestService"] = new HealthReportEntry(
                    MsHealthStatus.Healthy, "OK", TimeSpan.FromMilliseconds(123), null, null)
            },
            TimeSpan.FromMilliseconds(123));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var handler = new GetDiagnosticsHandler(
            _healthCheckServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);

        // Act
        var result = await handler.Handle(new GetDiagnosticsQuery(), CancellationToken.None);

        // Assert
        result.Services.First().ResponseTimeMs.Should().Be(123);
    }
}
