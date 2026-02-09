using FluentValidation;
using Po.Joker.DTOs;

namespace Po.Joker.Validation;

/// <summary>
/// Validator for JokeDto to ensure joke data integrity.
/// </summary>
public sealed class JokeDtoValidator : AbstractValidator<JokeDto>
{
    public JokeDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Joke ID must be a positive integer");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Joke category is required");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t is "twopart" or "single")
            .WithMessage("Joke type must be 'twopart' or 'single'");

        // For two-part jokes, setup and punchline are required
        When(x => x.Type == "twopart", () =>
        {
            RuleFor(x => x.Setup)
                .NotEmpty()
                .WithMessage("Setup is required for two-part jokes")
                .MaximumLength(1000)
                .WithMessage("Setup must not exceed 1000 characters");

            RuleFor(x => x.Punchline)
                .NotEmpty()
                .WithMessage("Punchline is required for two-part jokes")
                .MaximumLength(1000)
                .WithMessage("Punchline must not exceed 1000 characters");
        });

        // For single jokes, the joke text is required
        When(x => x.Type == "single", () =>
        {
            RuleFor(x => x.Joke)
                .NotEmpty()
                .WithMessage("Joke text is required for single jokes")
                .MaximumLength(2000)
                .WithMessage("Joke text must not exceed 2000 characters");
        });
    }
}

/// <summary>
/// Validator for JesterSettingsDto.
/// </summary>
public sealed class JesterSettingsDtoValidator : AbstractValidator<JesterSettingsDto>
{
    public JesterSettingsDtoValidator()
    {
        RuleFor(x => x.LoopIntervalSeconds)
            .InclusiveBetween(5, 120)
            .WithMessage("Loop interval must be between 5 and 120 seconds");

        RuleFor(x => x.PunchlineDelayMs)
            .InclusiveBetween(500, 10000)
            .WithMessage("Punchline delay must be between 500ms and 10 seconds");

        RuleFor(x => x.TtsRate)
            .InclusiveBetween(0.5, 2.0)
            .WithMessage("TTS rate must be between 0.5 and 2.0");

        RuleFor(x => x.TtsPitch)
            .InclusiveBetween(0.5, 2.0)
            .WithMessage("TTS pitch must be between 0.5 and 2.0");

        RuleFor(x => x.JokeTypes)
            .NotEmpty()
            .WithMessage("At least one joke type must be selected")
            .Must(types => types.All(t => t is "twopart" or "single"))
            .WithMessage("Joke types must be 'twopart' or 'single'");
    }
}
