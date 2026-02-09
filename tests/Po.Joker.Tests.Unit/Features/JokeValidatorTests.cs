using FluentAssertions;
using FluentValidation.TestHelper;
using Po.Joker.Shared.DTOs;
using Po.Joker.Shared.Validation;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// Unit tests for JokeDtoValidator and JesterSettingsDtoValidator.
/// Ensures FluentValidation rules guard API input boundaries.
/// </summary>
public sealed class JokeValidatorTests
{
    private readonly JokeDtoValidator _jokeValidator = new();
    private readonly JesterSettingsDtoValidator _settingsValidator = new();

    // ── JokeDtoValidator ──

    [Fact]
    public void JokeDto_ValidTwoPart_PassesValidation()
    {
        var joke = new JokeDto
        {
            Id = 1, Category = "Programming", Type = "twopart",
            Setup = "Why?", Punchline = "Because!"
        };
        _jokeValidator.TestValidate(joke).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void JokeDto_ValidSingle_PassesValidation()
    {
        var joke = new JokeDto
        {
            Id = 1, Category = "Pun", Type = "single",
            Joke = "A standalone joke."
        };
        _jokeValidator.TestValidate(joke).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void JokeDto_InvalidId_FailsValidation(int id)
    {
        var joke = new JokeDto { Id = id, Category = "Test", Type = "twopart", Setup = "S", Punchline = "P" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void JokeDto_EmptyCategory_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "", Type = "twopart", Setup = "S", Punchline = "P" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void JokeDto_InvalidType_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "Test", Type = "invalid" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void JokeDto_TwoPart_EmptySetup_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "Test", Type = "twopart", Setup = "", Punchline = "P" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Setup);
    }

    [Fact]
    public void JokeDto_TwoPart_EmptyPunchline_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "Test", Type = "twopart", Setup = "S", Punchline = "" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Punchline);
    }

    [Fact]
    public void JokeDto_Single_EmptyJokeText_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "Test", Type = "single", Joke = "" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Joke);
    }

    [Fact]
    public void JokeDto_TwoPart_SetupExceeds1000Chars_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "Test", Type = "twopart", Setup = new string('x', 1001), Punchline = "P" };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Setup);
    }

    [Fact]
    public void JokeDto_Single_JokeExceeds2000Chars_FailsValidation()
    {
        var joke = new JokeDto { Id = 1, Category = "Test", Type = "single", Joke = new string('x', 2001) };
        _jokeValidator.TestValidate(joke).ShouldHaveValidationErrorFor(x => x.Joke);
    }

    // ── JesterSettingsDtoValidator ──

    [Fact]
    public void Settings_DefaultValues_PassesValidation()
    {
        var settings = new JesterSettingsDto();
        _settingsValidator.TestValidate(settings).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(121)]
    public void Settings_LoopIntervalOutOfRange_FailsValidation(int interval)
    {
        var settings = new JesterSettingsDto { LoopIntervalSeconds = interval };
        _settingsValidator.TestValidate(settings).ShouldHaveValidationErrorFor(x => x.LoopIntervalSeconds);
    }

    [Theory]
    [InlineData(499)]
    [InlineData(10001)]
    public void Settings_PunchlineDelayOutOfRange_FailsValidation(int delay)
    {
        var settings = new JesterSettingsDto { PunchlineDelayMs = delay };
        _settingsValidator.TestValidate(settings).ShouldHaveValidationErrorFor(x => x.PunchlineDelayMs);
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(2.1)]
    public void Settings_TtsRateOutOfRange_FailsValidation(double rate)
    {
        var settings = new JesterSettingsDto { TtsRate = rate };
        _settingsValidator.TestValidate(settings).ShouldHaveValidationErrorFor(x => x.TtsRate);
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(2.1)]
    public void Settings_TtsPitchOutOfRange_FailsValidation(double pitch)
    {
        var settings = new JesterSettingsDto { TtsPitch = pitch };
        _settingsValidator.TestValidate(settings).ShouldHaveValidationErrorFor(x => x.TtsPitch);
    }

    [Fact]
    public void Settings_EmptyJokeTypes_FailsValidation()
    {
        var settings = new JesterSettingsDto { JokeTypes = [] };
        _settingsValidator.TestValidate(settings).ShouldHaveValidationErrorFor(x => x.JokeTypes);
    }

    [Fact]
    public void Settings_InvalidJokeType_FailsValidation()
    {
        var settings = new JesterSettingsDto { JokeTypes = ["unknown"] };
        _settingsValidator.TestValidate(settings).ShouldHaveValidationErrorFor(x => x.JokeTypes);
    }
}
