using Bunit;
using Po.Joker.Client.Components;
using Po.Joker.Shared.DTOs;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// TDD tests for JokeCard.razor component - Written FIRST before implementation.
/// Tests joke display with setup/guess/delivery states.
/// </summary>
public class JokeCardTests : BunitContext
{
    [Fact]
    public void JokeCard_DisplaysSetup_WhenProvided()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Programming",
            Type = "twopart",
            Setup = "Why do Java developers wear glasses?",
            Punchline = "Because they don't C#!"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true));

        // Assert
        cut.Markup.Should().Contain("Why do Java developers wear glasses?");
        cut.Find(".joke-setup").Should().NotBeNull();
    }

    [Fact]
    public void JokeCard_HidesPunchline_WhenNotRevealed()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Programming",
            Type = "twopart",
            Setup = "Test setup",
            Punchline = "Secret punchline"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true)
            .Add(p => p.ShowPunchline, false));

        // Assert
        cut.Markup.Should().NotContain("Secret punchline");
    }

    [Fact]
    public void JokeCard_RevealsPunchline_WhenShowPunchlineIsTrue()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Pun",
            Type = "twopart",
            Setup = "What do you call a fake noodle?",
            Punchline = "An impasta!"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true)
            .Add(p => p.ShowPunchline, true));

        // Assert
        cut.Markup.Should().Contain("An impasta!");
        cut.Find(".joke-punchline").Should().NotBeNull();
    }

    [Fact]
    public void JokeCard_DisplaysAiGuess_WhenProvided()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Programming",
            Type = "twopart",
            Setup = "Test",
            Punchline = "Actual answer"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true)
            .Add(p => p.AiGuess, "The Jester's guess!"));

        // Assert
        cut.Markup.Should().Contain("The Jester's guess!");
        cut.Find(".ai-guess").Should().NotBeNull();
    }

    [Fact]
    public void JokeCard_DisplaysCategory_WhenShown()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Dark",
            Type = "twopart",
            Setup = "Test",
            Punchline = "Test"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true)
            .Add(p => p.ShowCategory, true));

        // Assert
        cut.Markup.Should().Contain("Dark");
    }

    [Fact]
    public void JokeCard_AppliesTriumphClass_WhenTriumph()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Pun",
            Type = "twopart",
            Setup = "Test",
            Punchline = "Test"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true)
            .Add(p => p.ShowPunchline, true)
            .Add(p => p.IsTriumph, true));

        // Assert
        cut.Markup.Should().Contain("triumph");
    }

    [Fact]
    public void JokeCard_AppliesDefeatClass_WhenDefeat()
    {
        // Arrange
        var joke = new JokeDto
        {
            Id = 1,
            Category = "Programming",
            Type = "twopart",
            Setup = "Test",
            Punchline = "Test"
        };

        // Act
        var cut = Render<JokeCard>(parameters => parameters
            .Add(p => p.Joke, joke)
            .Add(p => p.ShowSetup, true)
            .Add(p => p.ShowPunchline, true)
            .Add(p => p.IsTriumph, false)
            .Add(p => p.ShowResult, true));

        // Assert
        cut.Markup.Should().Contain("defeat");
    }
}
