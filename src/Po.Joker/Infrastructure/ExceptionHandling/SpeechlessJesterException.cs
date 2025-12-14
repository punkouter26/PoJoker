namespace Po.Joker.Infrastructure.ExceptionHandling;

/// <summary>
/// Exception thrown when Azure OpenAI content filters block a request.
/// Triggers the "Speechless Jester" protocol with themed messaging.
/// </summary>
public sealed class SpeechlessJesterException : Exception
{
    /// <summary>
    /// The category of content that was filtered.
    /// </summary>
    public string? FilterCategory { get; }

    /// <summary>
    /// The severity level of the content filter trigger.
    /// </summary>
    public string? Severity { get; }

    /// <summary>
    /// The joke ID that triggered the filter (if available).
    /// </summary>
    public int? JokeId { get; }

    public SpeechlessJesterException()
        : base("The Court forbids this tongue! The Jester has been silenced.")
    {
    }

    public SpeechlessJesterException(string message)
        : base(message)
    {
    }

    public SpeechlessJesterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public SpeechlessJesterException(string message, string? filterCategory, string? severity, int? jokeId = null)
        : base(message)
    {
        FilterCategory = filterCategory;
        Severity = severity;
        JokeId = jokeId;
    }

    /// <summary>
    /// Creates a SpeechlessJesterException from an Azure OpenAI content filter response.
    /// </summary>
    public static SpeechlessJesterException FromContentFilter(string? category = null, string? severity = null, int? jokeId = null)
    {
        var message = category switch
        {
            "hate" => "The Court forbids such hateful speech! The Jester must hold his tongue.",
            "violence" => "The Court forbids tales of violence! The Jester retreats in silence.",
            "sexual" => "The Court deems this too bawdy! The Jester blushes and falls silent.",
            "self_harm" => "The Court protects all in the realm! The Jester chooses kinder words.",
            _ => "The Court forbids this tongue! The Jester has been silenced by royal decree."
        };

        return new SpeechlessJesterException(message, category, severity, jokeId);
    }
}
