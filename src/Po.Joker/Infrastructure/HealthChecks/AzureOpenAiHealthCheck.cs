using Azure.AI.OpenAI;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenAI.Chat;

namespace Po.Joker.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Azure OpenAI service connectivity.
/// </summary>
public sealed class AzureOpenAiHealthCheck : IHealthCheck
{
    private readonly AzureOpenAIClient? _openAiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureOpenAiHealthCheck> _logger;

    public AzureOpenAiHealthCheck(
        IConfiguration configuration,
        ILogger<AzureOpenAiHealthCheck> logger,
        AzureOpenAIClient? openAiClient = null)
    {
        _openAiClient = openAiClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // If no OpenAI client is configured, report as degraded (not unhealthy)
        if (_openAiClient is null)
        {
            _logger.LogInformation("Azure OpenAI client not configured - skipping health check");
            return HealthCheckResult.Degraded("Azure OpenAI not configured. AI features unavailable.");
        }

        try
        {
            var deploymentName = _configuration["Azure:OpenAI:DeploymentName"] ?? "gpt-4o-mini";
            var chatClient = _openAiClient.GetChatClient(deploymentName);

            // Send a minimal request to verify connectivity
            var messages = new ChatMessage[]
            {
                new SystemChatMessage("Reply with exactly: OK"),
                new UserChatMessage("Health check")
            };

            var response = await chatClient.CompleteChatAsync(
                messages,
                new ChatCompletionOptions { MaxOutputTokenCount = 5 },
                cancellationToken);

            if (response.Value.Content.Count > 0)
            {
                return HealthCheckResult.Healthy("Azure OpenAI is responding normally.");
            }

            return HealthCheckResult.Degraded("Azure OpenAI returned empty response.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Azure OpenAI health check failed");
            return HealthCheckResult.Unhealthy("Unable to reach Azure OpenAI.", ex);
        }
    }
}
