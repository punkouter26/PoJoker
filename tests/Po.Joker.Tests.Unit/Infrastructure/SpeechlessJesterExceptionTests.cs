using FluentAssertions;
using Po.Joker.Infrastructure.ExceptionHandling;

namespace Po.Joker.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for SpeechlessJesterException.
/// </summary>
public sealed class SpeechlessJesterExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        // Act
        var exception = new SpeechlessJesterException();

        // Assert
        exception.Message.Should().Contain("Court forbids");
    }

    [Fact]
    public void MessageConstructor_SetsCustomMessage()
    {
        // Arrange
        var message = "Custom speechless message";

        // Act
        var exception = new SpeechlessJesterException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_SetsProperties()
    {
        // Arrange
        var message = "Outer message";
        var inner = new InvalidOperationException("Inner exception");

        // Act
        var exception = new SpeechlessJesterException(message, inner);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(inner);
    }

    [Fact]
    public void FullConstructor_SetsAllProperties()
    {
        // Arrange
        var message = "Test message";
        var category = "hate";
        var severity = "high";
        var jokeId = 42;

        // Act
        var exception = new SpeechlessJesterException(message, category, severity, jokeId);

        // Assert
        exception.Message.Should().Be(message);
        exception.FilterCategory.Should().Be(category);
        exception.Severity.Should().Be(severity);
        exception.JokeId.Should().Be(jokeId);
    }

    [Fact]
    public void FromContentFilter_WithHateCategory_ReturnsAppropriateMessage()
    {
        // Act
        var exception = SpeechlessJesterException.FromContentFilter("hate");

        // Assert
        exception.Message.Should().Contain("hateful speech");
        exception.FilterCategory.Should().Be("hate");
    }

    [Fact]
    public void FromContentFilter_WithViolenceCategory_ReturnsAppropriateMessage()
    {
        // Act
        var exception = SpeechlessJesterException.FromContentFilter("violence");

        // Assert
        exception.Message.Should().Contain("violence");
        exception.FilterCategory.Should().Be("violence");
    }

    [Fact]
    public void FromContentFilter_WithSexualCategory_ReturnsAppropriateMessage()
    {
        // Act
        var exception = SpeechlessJesterException.FromContentFilter("sexual");

        // Assert
        exception.Message.Should().Contain("bawdy");
        exception.FilterCategory.Should().Be("sexual");
    }

    [Fact]
    public void FromContentFilter_WithSelfHarmCategory_ReturnsAppropriateMessage()
    {
        // Act
        var exception = SpeechlessJesterException.FromContentFilter("self_harm");

        // Assert
        exception.Message.Should().Contain("protects");
        exception.FilterCategory.Should().Be("self_harm");
    }

    [Fact]
    public void FromContentFilter_WithUnknownCategory_ReturnsDefaultMessage()
    {
        // Act
        var exception = SpeechlessJesterException.FromContentFilter("unknown");

        // Assert
        exception.Message.Should().Contain("royal decree");
        exception.FilterCategory.Should().Be("unknown");
    }

    [Fact]
    public void FromContentFilter_WithNoCategory_ReturnsDefaultMessage()
    {
        // Act
        var exception = SpeechlessJesterException.FromContentFilter();

        // Assert
        exception.Message.Should().Contain("Court forbids");
        exception.FilterCategory.Should().BeNull();
    }

    [Fact]
    public void FromContentFilter_WithJokeId_SetsJokeId()
    {
        // Arrange
        var jokeId = 123;

        // Act
        var exception = SpeechlessJesterException.FromContentFilter(jokeId: jokeId);

        // Assert
        exception.JokeId.Should().Be(jokeId);
    }

    [Fact]
    public void FromContentFilter_WithSeverity_SetsSeverity()
    {
        // Arrange
        var severity = "high";

        // Act
        var exception = SpeechlessJesterException.FromContentFilter(severity: severity);

        // Assert
        exception.Severity.Should().Be(severity);
    }
}
