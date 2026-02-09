using System.Text.Json;
using System.Text.Json.Serialization;
using Po.Joker.DTOs;

namespace Po.Joker.Features.Jokes;

/// <summary>
/// HTTP client for fetching jokes from JokeAPI.dev
/// Supports safe mode filtering and duplicate exclusion.
/// </summary>
public sealed class JokeApiClient : IJokeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JokeApiClient> _logger;
    private const string BaseUrl = "https://v2.jokeapi.dev/joke";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public JokeApiClient(HttpClient httpClient, ILogger<JokeApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<JokeDto> FetchJokeAsync(
        bool safeMode = true,
        IEnumerable<int>? excludeIds = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(safeMode, excludeIds);
        _logger.LogDebug("Fetching joke from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var jokeResponse = JsonSerializer.Deserialize<JokeApiResponse>(content, JsonOptions);

        if (jokeResponse is null || jokeResponse.Error)
        {
            throw new InvalidOperationException($"JokeAPI returned error: {jokeResponse?.Message ?? "Unknown error"}");
        }

        return new JokeDto
        {
            Id = jokeResponse.Id,
            Category = jokeResponse.Category ?? "Unknown",
            Type = jokeResponse.Type ?? "twopart",
            Setup = jokeResponse.Setup ?? string.Empty,
            Punchline = jokeResponse.Delivery ?? string.Empty,
            SafeMode = safeMode,
            Flags = new JokeFlags
            {
                Nsfw = jokeResponse.Flags?.Nsfw ?? false,
                Religious = jokeResponse.Flags?.Religious ?? false,
                Political = jokeResponse.Flags?.Political ?? false,
                Racist = jokeResponse.Flags?.Racist ?? false,
                Sexist = jokeResponse.Flags?.Sexist ?? false,
                Explicit = jokeResponse.Flags?.Explicit ?? false
            }
        };
    }

    private static string BuildUrl(bool safeMode, IEnumerable<int>? excludeIds)
    {
        // Request only two-part jokes from all categories
        var url = $"{BaseUrl}/Any?type=twopart";

        // Apply safe mode filters
        if (safeMode)
        {
            url += "&safe-mode";
        }

        // Note: JokeAPI does not support excluding specific joke IDs via API.
        // Exclusion is handled at the application level by re-fetching if needed.
        // The excludeIds parameter is passed through for higher-level filtering.

        return url;
    }

    private sealed record JokeApiResponse
    {
        public bool Error { get; init; }
        public string? Message { get; init; }
        public int Id { get; init; }
        public string? Category { get; init; }
        public string? Type { get; init; }
        public string? Setup { get; init; }
        public string? Delivery { get; init; }
        public JokeApiFlags? Flags { get; init; }
    }

    private sealed record JokeApiFlags
    {
        public bool Nsfw { get; init; }
        public bool Religious { get; init; }
        public bool Political { get; init; }
        public bool Racist { get; init; }
        public bool Sexist { get; init; }
        public bool Explicit { get; init; }
    }
}
