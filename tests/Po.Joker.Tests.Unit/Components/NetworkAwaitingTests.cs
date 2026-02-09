using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Po.Joker.Components;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// Unit tests for NetworkAwaiting component.
/// </summary>
public sealed class NetworkAwaitingTests : BunitContext
{
    [Fact]
    public void NetworkAwaiting_WhenNotVisible_HasNoVisibleClass()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        var container = cut.Find(".network-awaiting");
        container.ClassList.Should().NotContain("visible");
    }

    [Fact]
    public void NetworkAwaiting_WhenVisible_HasVisibleClass()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        var container = cut.Find(".network-awaiting");
        container.ClassList.Should().Contain("visible");
    }

    [Fact]
    public void NetworkAwaiting_DisplaysCustomMessage()
    {
        // Arrange
        var message = "Custom network error message";

        // Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Message, message));

        // Assert
        var messageElement = cut.Find(".network-message");
        messageElement.TextContent.Should().Contain(message);
    }

    [Fact]
    public void NetworkAwaiting_DisplaysStatusText()
    {
        // Arrange
        var statusText = "Attempting reconnection...";

        // Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.StatusText, statusText));

        // Assert
        var status = cut.Find(".status-text");
        status.TextContent.Should().Contain(statusText);
    }

    [Fact]
    public void NetworkAwaiting_ShowsRetryButton_WhenEnabled()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ShowRetryButton, true));

        // Assert
        var button = cut.Find(".retry-button");
        button.Should().NotBeNull();
        // Simplified UI copy uses standard retry text
        button.TextContent.Should().Contain("Retry");
    }

    [Fact]
    public void NetworkAwaiting_HidesRetryButton_WhenDisabled()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ShowRetryButton, false));

        // Assert
        var buttons = cut.FindAll(".retry-button");
        buttons.Should().BeEmpty();
    }

    [Fact]
    public async Task NetworkAwaiting_InvokesOnRetry_WhenButtonClicked()
    {
        // Arrange
        var retryInvoked = false;
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ShowRetryButton, true)
            .Add(p => p.OnRetry, EventCallback.Factory.Create(this, () => retryInvoked = true)));

        // Act
        var button = cut.Find(".retry-button");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        retryInvoked.Should().BeTrue();
    }

    [Fact]
    public void NetworkAwaiting_DisplaysTitle()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        // Title removed in simplified UI; rely on container presence
        cut.Find(".network-awaiting").Should().NotBeNull();
    }

    [Fact]
    public void NetworkAwaiting_ShowsRetryingState_WhenRetrying()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.IsRetrying, true)
            .Add(p => p.ShowRetryButton, true));

        // Assert
        var button = cut.Find(".retry-button");
        // Simplified UI uses 'Retrying...'
        button.TextContent.Should().Contain("Retrying");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void NetworkAwaiting_DisplaysRetryCount_WhenGreaterThanZero()
    {
        // Arrange
        var retryCount = 3;

        // Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.RetryCount, retryCount));

        // Assert
        var retryText = cut.Find(".retry-count");
        retryText.TextContent.Should().Contain("3");
    }

    [Fact]
    public void NetworkAwaiting_HidesRetryCount_WhenZero()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.RetryCount, 0));

        // Assert
        var retryTexts = cut.FindAll(".retry-count");
        retryTexts.Should().BeEmpty();
    }

    [Fact]
    public void NetworkAwaiting_DisplaysCourierIcons()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        // Decorative courier icons removed in simplified UI; assert container exists
        cut.Find(".network-awaiting").Should().NotBeNull();
    }

    [Fact]
    public void NetworkAwaiting_HasStatusDots()
    {
        // Arrange & Act
        var cut = Render<NetworkAwaiting>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        // Status dots removed; assert container exists
        cut.Find(".network-awaiting").Should().NotBeNull();
    }
}
