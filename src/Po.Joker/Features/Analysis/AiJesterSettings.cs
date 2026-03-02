namespace Po.Joker.Features.Analysis;

/// <summary>
/// Settings for AI Jester service.
/// </summary>
public sealed class AiJesterSettings
{
    public string DeploymentName { get; init; } = "gpt-4o-mini";
    public int OpenAiTimeoutSeconds { get; init; } = 30;
}
