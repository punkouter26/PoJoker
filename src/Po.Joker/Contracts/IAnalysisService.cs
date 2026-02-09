using Po.Joker.DTOs;

namespace Po.Joker.Contracts;

/// <summary>
/// Service contract for AI joke analysis and punchline prediction.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Predicts the punchline for a given joke setup.
    /// </summary>
    /// <param name="joke">The joke to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Analysis result with predicted punchline and confidence.</returns>
    Task<JokeAnalysisDto> PredictPunchlineAsync(
        JokeDto joke,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rates a joke across multiple dimensions.
    /// </summary>
    /// <param name="joke">The joke to rate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rating scores and commentary.</returns>
    Task<JokeRatingDto> RateJokeAsync(
        JokeDto joke,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs full analysis: prediction and rating in one call.
    /// </summary>
    /// <param name="joke">The joke to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete analysis with prediction, rating, and commentary.</returns>
    Task<(JokeAnalysisDto Analysis, JokeRatingDto Rating)> AnalyzeJokeAsync(
        JokeDto joke,
        CancellationToken cancellationToken = default);
}
