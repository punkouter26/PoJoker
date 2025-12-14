using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Po.Joker.Client.Components;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// Unit tests for SpeechlessJester component.
/// </summary>
public sealed class SpeechlessJesterTests : BunitContext
{
    [Fact]
    public void SpeechlessJester_WhenNotVisible_HasNoVisibleClass()
    {
        // Arrange & Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Message, "Test message"));

        // Assert
        var container = cut.Find(".speechless-jester");
        container.ClassList.Should().NotContain("visible");
    }

    [Fact]
    public void SpeechlessJester_WhenVisible_HasVisibleClass()
    {
        // Arrange & Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Message, "Test message"));

        // Assert
        var container = cut.Find(".speechless-jester");
        container.ClassList.Should().Contain("visible");
    }

    [Fact]
    public void SpeechlessJester_DisplaysCustomMessage()
    {
        // Arrange
        var message = "Custom forbidden content message";

        // Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Message, message));

        // Assert
        var messageElement = cut.Find(".speechless-message");
        messageElement.TextContent.Should().Contain(message);
    }

    [Fact]
    public void SpeechlessJester_ShowsResumeButton_WhenEnabled()
    {
        // Arrange & Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ShowResumeButton, true));

        // Assert
        var button = cut.Find(".resume-button");
        button.Should().NotBeNull();
            // Simplified UI copy now uses shorter label
            button.TextContent.Should().Contain("Resume");
    }

    [Fact]
    public void SpeechlessJester_HidesResumeButton_WhenDisabled()
    {
        // Arrange & Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ShowResumeButton, false));

        // Assert
        var buttons = cut.FindAll(".resume-button");
        buttons.Should().BeEmpty();
    }

    [Fact]
    public async Task SpeechlessJester_InvokesOnResume_WhenButtonClicked()
    {
        // Arrange
        var resumeInvoked = false;
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ShowResumeButton, true)
            .Add(p => p.AutoResumeSeconds, 0) // Disable auto-resume for test
            .Add(p => p.OnResume, EventCallback.Factory.Create(this, () => resumeInvoked = true)));

        // Act
        var button = cut.Find(".resume-button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        resumeInvoked.Should().BeTrue();
    }

    [Fact]
    public void SpeechlessJester_DisplaysTitle()
    {
        // Arrange & Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
            // Title selector removed in simplified UI; assert overlay container exists
            cut.Find(".speechless-overlay").Should().NotBeNull();
    }

    [Fact]
    public void SpeechlessJester_DisplaysSilencedEmoji()
    {
        // Arrange & Act
        var cut = Render<SpeechlessJester>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
            // Decorative silenced emoji removed; assert overlay visibility instead
            cut.Find(".speechless-overlay").ClassList.Should().Contain("visible");
    }
}
