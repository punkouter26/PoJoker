using System.Text.Json;
using System.Text.Json.Serialization;

namespace Po.Joker.Features.Diagnostics;

/// <summary>
/// DTO representing a single configuration entry with masked value for security.
/// Follows the rule: "All apps should have /diag page that exposes all connection strings,
/// keys, values, secrets in json format / hide middle of values for security."
/// </summary>
public sealed record DiagConfigEntryDto
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }
}

/// <summary>
/// Aggregated diagnostics configuration payload returned by GET /diag.
/// </summary>
public sealed record DiagConfigDto
{
    [JsonPropertyName("environment")]
    public required string Environment { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("entries")]
    public IReadOnlyList<DiagConfigEntryDto> Entries { get; init; } = [];
}

/// <summary>
/// Helper utilities for masking sensitive configuration values.
/// Shows the first and last characters while hiding the middle portion.
/// </summary>
public static class ConfigMasker
{
    /// <summary>
    /// Masks the middle portion of a value for security display.
    /// Short values (≤4 chars) are fully masked. Otherwise first 2 and last 2 chars are visible.
    /// </summary>
    public static string Mask(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "(empty)";

        if (value.Length <= 4)
            return new string('*', value.Length);

        var visiblePrefix = value[..2];
        var visibleSuffix = value[^2..];
        var maskedMiddle = new string('*', Math.Min(value.Length - 4, 20));

        return $"{visiblePrefix}{maskedMiddle}{visibleSuffix}";
    }
}
