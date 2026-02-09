using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Po.Joker.Components;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// TDD tests for CommentaryBubble.razor component.
/// Tests display of AI Jester's personality-driven commentary.
/// </summary>
public class CommentaryBubbleTests : BunitContext
{
    [Fact]
    public void CommentaryBubble_ShouldDisplayCommentary()
    {
        // Arrange
        var commentary = "Verily, this jest doth make mine sides split with mirth!";

        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, commentary));

        // Assert
        cut.Markup.Should().Contain("Verily");
        cut.Markup.Should().Contain("mirth");
    }

    [Fact]
    public void CommentaryBubble_ShouldHaveSpeechBubbleClass()
    {
        // Arrange
        var commentary = "A most clever wordplay, I must say!";

        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, commentary));

        // Assert
        cut.Find(".commentary-bubble").Should().NotBeNull();
    }

    [Fact]
    public void CommentaryBubble_ShouldDisplayJesterIcon()
    {
        // Arrange
        var commentary = "The court erupts in laughter!";

        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, commentary));

        // Assert
        // Should have jester emoji or icon
        var markup = cut.Markup;
        (markup.Contains("🎭") || markup.Contains("🃏") || markup.Contains("jester")).Should().BeTrue();
    }

    [Fact]
    public void CommentaryBubble_ShouldHandleEmptyCommentary()
    {
        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, string.Empty));

        // Assert - Should render but be empty or show placeholder
        cut.Find(".commentary-bubble").Should().NotBeNull();
    }

    [Fact]
    public void CommentaryBubble_ShouldHandleNullCommentary()
    {
        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, null));

        // Assert - Should not throw, render gracefully
        cut.Markup.Should().NotBeNull();
    }

    [Fact]
    public void CommentaryBubble_ShouldSupportTriumphState()
    {
        // Arrange
        var commentary = "TRIUMPH! The Jester has bested the jest!";

        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, commentary)
            .Add(p => p.IsTriumph, true));

        // Assert
        cut.Find(".commentary-bubble").ClassList.Should().Contain("triumph");
    }

    [Fact]
    public void CommentaryBubble_ShouldSupportDefeatState()
    {
        // Arrange
        var commentary = "Alas! The jest has proven too cunning this day.";

        // Act
        var cut = Render<CommentaryBubble>(parameters => parameters
            .Add(p => p.Commentary, commentary)
            .Add(p => p.IsTriumph, false));

        // Assert
        cut.Find(".commentary-bubble").ClassList.Should().Contain("defeat");
    }
}
