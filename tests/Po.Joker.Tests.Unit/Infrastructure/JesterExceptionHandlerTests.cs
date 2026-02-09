using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Po.Joker.Infrastructure.ExceptionHandling;

namespace Po.Joker.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for JesterExceptionHandler.
/// Verifies exception-to-ProblemDetails mapping for all exception types.
/// </summary>
public sealed class JesterExceptionHandlerTests
{
    private readonly JesterExceptionHandler _handler;
    private readonly Mock<IHostEnvironment> _environment;

    public JesterExceptionHandlerTests()
    {
        var logger = Mock.Of<ILogger<JesterExceptionHandler>>();
        _environment = new Mock<IHostEnvironment>();
        _environment.Setup(e => e.EnvironmentName).Returns("Production");
        _handler = new JesterExceptionHandler(logger, _environment.Object);
    }

    [Theory]
    [InlineData(typeof(ArgumentNullException), 400)]
    [InlineData(typeof(ArgumentException), 400)]
    [InlineData(typeof(InvalidOperationException), 400)]
    [InlineData(typeof(HttpRequestException), 503)]
    [InlineData(typeof(TaskCanceledException), 408)]
    [InlineData(typeof(TimeoutException), 408)]
    [InlineData(typeof(UnauthorizedAccessException), 401)]
    [InlineData(typeof(KeyNotFoundException), 404)]
    [InlineData(typeof(NotImplementedException), 501)]
    public async Task TryHandleAsync_MapsExceptionToCorrectStatusCode(Type exceptionType, int expectedStatusCode)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test")!;
        var httpContext = CreateHttpContext();

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
        httpContext.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500()
    {
        // Arrange
        var exception = new Exception("unexpected");
        var httpContext = CreateHttpContext();

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_SpeechlessJesterException_Returns451()
    {
        // Arrange
        var exception = new SpeechlessJesterException("Silenced!", "hate", "high", 42);
        var httpContext = CreateHttpContext();

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(451);
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_ExposesExceptionMessage()
    {
        // Arrange
        _environment.Setup(e => e.EnvironmentName).Returns("Development");
        var devHandler = new JesterExceptionHandler(
            Mock.Of<ILogger<JesterExceptionHandler>>(), _environment.Object);
        var exception = new ArgumentException("Sensitive debug info");
        var httpContext = CreateHttpContext();

        // Act
        await devHandler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert - read response body to verify detail contains the exception message
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        body.Should().Contain("Sensitive debug info");
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_HidesExceptionMessage()
    {
        // Arrange
        var exception = new ArgumentException("Sensitive internal detail");
        var httpContext = CreateHttpContext();

        // Act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        body.Should().NotContain("Sensitive internal detail");
        body.Should().Contain("invalid"); // The themed user-facing message
    }

    [Fact]
    public async Task TryHandleAsync_SetsTraceIdExtension()
    {
        // Arrange
        var exception = new Exception("test");
        var httpContext = CreateHttpContext();
        httpContext.TraceIdentifier = "test-trace-123";

        // Act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        body.Should().Contain("test-trace-123");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";
        return context;
    }
}
