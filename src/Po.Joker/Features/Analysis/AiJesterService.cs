using System.Diagnostics;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Po.Joker.Infrastructure.ExceptionHandling;
using Po.Joker.Infrastructure.Telemetry;
using Po.Joker.Contracts;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Analysis;

/// <summary>
/// Settings for AI Jester service.
/// </summary>
public sealed class AiJesterSettings
{
    public string DeploymentName { get; init; } = "gpt-4.1-nano";
}

/// <summary>
/// AI-powered joke analysis service using Azure OpenAI.
/// Predicts punchlines and calculates triumph/defeat.
/// </summary>
public sealed class AiJesterService : IAnalysisService
{
    private readonly AzureOpenAIClient _openAiClient;
    private readonly ILogger<AiJesterService> _logger;
    private readonly string _deploymentName;

    private const string SystemPrompt = """
        You are a Digital Jester - an AI that tries to predict punchlines to jokes.
        Given a joke setup, you must predict what the punchline will be.
        Be creative and funny, but try to guess the actual punchline.
        Respond with ONLY the punchline, nothing else. No explanations, no "I think", just the punchline itself.
        Keep your response short and punchy.
        """;

    public AiJesterService(
        AzureOpenAIClient openAiClient,
        ILogger<AiJesterService> logger,
        AiJesterSettings settings)
    {
        _openAiClient = openAiClient;
        _logger = logger;
        _deploymentName = settings.DeploymentName;
    }

    public async Task<JokeAnalysisDto> PredictPunchlineAsync(JokeDto joke, CancellationToken cancellationToken = default)

    {
        using var activity = OpenTelemetryConfig.ActivitySource.StartActivity("AI.PredictPunchline");
        activity?.SetTag("joke.id", joke.Id);
        activity?.SetTag("joke.category", joke.Category);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var chatClient = _openAiClient.GetChatClient(_deploymentName);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage($"Joke setup: \"{joke.Setup}\"")
            };
            var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

            // Check for content filter triggering

            string aiPunchline;
            if (response.Value.FinishReason == ChatFinishReason.ContentFilter)
            {
                _logger.LogWarning("Content filter triggered for joke {JokeId}, returning fallback punchline.", joke.Id);
                aiPunchline = "[The Jester shrugs and delivers a safe, court-approved punchline.]";
            }
            else
            {
                aiPunchline = response.Value.Content[0].Text.Trim();
            }
            stopwatch.Stop();
            var similarityScore = CalculateSimilarity(joke.Punchline, aiPunchline);
            var isTriumph = similarityScore >= 0.55 && response.Value.FinishReason != ChatFinishReason.ContentFilter;
            OpenTelemetryConfig.JokesAnalyzed.Add(1,
                new KeyValuePair<string, object?>("category", joke.Category),
                new KeyValuePair<string, object?>("is_triumph", isTriumph));
            if (isTriumph)
            {
                OpenTelemetryConfig.AiTriumphs.Add(1);
            }
            _logger.LogInformation(
                "AI predicted punchline for joke {JokeId}: Similarity={Similarity:P1}, IsTriumph={IsTriumph}",
                joke.Id, similarityScore, isTriumph);
            return new JokeAnalysisDto
            {
                OriginalJoke = joke,
                AiPunchline = aiPunchline,
                Confidence = response.Value.FinishReason == ChatFinishReason.Stop ? 0.9 : 0.5,
                IsTriumph = isTriumph,
                SimilarityScore = similarityScore,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "AI prediction failed for joke {JokeId}", joke.Id);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        throw new InvalidOperationException("Unreachable code in PredictPunchlineAsync");
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        int[,] dp = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            dp[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                if (s1[i - 1] == s2[j - 1])
                    dp[i, j] = dp[i - 1, j - 1];
                else
                    dp[i, j] = 1 + Math.Min(
                        Math.Min(dp[i - 1, j], dp[i, j - 1]),
                        dp[i - 1, j - 1]);
            }
        }

        return dp[s1.Length, s2.Length];
    }

    private static double LevenshteinSimilarity(string s1, string s2)
    {
        int maxLength = Math.Max(s1.Length, s2.Length);
        if (maxLength == 0)
            return 1.0;

        int distance = LevenshteinDistance(s1, s2);
        return 1.0 - ((double)distance / maxLength);
    }

    private static double CalculateSimilarity(string actual, string predicted)
    {
        if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(predicted))
            return 0;

        var actualLower = actual.ToLowerInvariant();
        var predictedLower = predicted.ToLowerInvariant();

        if (actualLower == predictedLower)
            return 1.0;

        var levenshteinSimilarity = LevenshteinSimilarity(actualLower, predictedLower);

        var actualWords = actualLower
            .Split(new[] { ' ', '.', '!', '?', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var predictedWords = predictedLower
            .Split(new[] { ' ', '.', '!', '?', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        double jaccardSimilarity = 0;
        if (actualWords.Count > 0 && predictedWords.Count > 0)
        {
            var intersection = actualWords.Intersect(predictedWords).Count();
            var union = actualWords.Union(predictedWords).Count();
            jaccardSimilarity = (double)intersection / union;
        }

        return (levenshteinSimilarity * 0.55) + (jaccardSimilarity * 0.45);
    }

    public async Task<JokeRatingDto> RateJokeAsync(JokeDto joke, CancellationToken cancellationToken = default)
    {
        using var activity = OpenTelemetryConfig.ActivitySource.StartActivity("AI.RateJoke");

        var chatClient = _openAiClient.GetChatClient(_deploymentName);

        var ratingPrompt = """
            Rate this joke on a scale of 0.0 to 1.0 for:
            - Originality: How unique and creative is it?
            - Cleverness: How smart or witty is the wordplay?
            - Humor: How funny is it overall?
            
            Respond in this exact format (just numbers, no text):
            originality: 0.X
            cleverness: 0.X
            humor: 0.X
            """;

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(ratingPrompt),
            new UserChatMessage($"Joke: \"{joke.Setup}\" -> \"{joke.Punchline}\"")
        };

        var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        // Check for content filter triggering
        if (response.Value.FinishReason == ChatFinishReason.ContentFilter)
        {
            _logger.LogWarning("Content filter triggered while rating joke {JokeId}, returning neutral scores.", joke.Id);
            // Return neutral/average scores if filtered
            return new JokeRatingDto
            {
                Cleverness = 5,
                Complexity = 5,
                Difficulty = 5,
                Rudeness = 1,
                Commentary = "Rated by the Digital Jester's discerning wit. (Filtered)"
            };
        }

        var responseText = response.Value.Content[0].Text;

        // Parse scores (simple parsing, convert to 1-10 scale)
        var cleverness = (int)Math.Round((ExtractScore(responseText, "cleverness") ?? 0.5) * 10);
        var complexity = (int)Math.Round((ExtractScore(responseText, "originality") ?? 0.5) * 10);
        var difficulty = (int)Math.Round((ExtractScore(responseText, "humor") ?? 0.5) * 10);

        return new JokeRatingDto
        {
            Cleverness = cleverness,
            Complexity = complexity,
            Difficulty = difficulty,
            Rudeness = 1, // Default low rudeness
            Commentary = "Rated by the Digital Jester's discerning wit."
        };
    }

    private static double? ExtractScore(string text, string key)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            if (line.ToLowerInvariant().Contains(key))
            {
                var parts = line.Split(':');
                if (parts.Length > 1 && double.TryParse(parts[1].Trim(), out var score))
                {
                    return Math.Clamp(score, 0, 1);
                }
            }
        }
        return null;
    }

    public async Task<(JokeAnalysisDto Analysis, JokeRatingDto Rating)> AnalyzeJokeAsync(JokeDto joke, CancellationToken cancellationToken = default)
    {
        var analysisTask = PredictPunchlineAsync(joke, cancellationToken);
        var ratingTask = RateJokeAsync(joke, cancellationToken);

        await Task.WhenAll(analysisTask, ratingTask);

        return (analysisTask.Result, ratingTask.Result);
    }
}
