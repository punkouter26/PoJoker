using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Po.Joker.Features.Analysis;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Contracts;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring Azure services.
/// </summary>
public static class AzureServicesConfiguration
{
    /// <summary>
    /// Adds Azure Key Vault configuration.
    /// </summary>
    public static IConfigurationBuilder AddPoJokerKeyVault(
        this IConfigurationBuilder configuration,
        IHostEnvironment environment)
    {
        // Allow disabling Key Vault via env var or appsettings for local runs and troubleshooting
        var disableKeyVault = Environment.GetEnvironmentVariable("POJOKER_DISABLE_KEYVAULT");
        if (string.IsNullOrEmpty(disableKeyVault))
        {
            disableKeyVault = configuration.Build()["POJOKER_DISABLE_KEYVAULT"];
        }
        if (!string.IsNullOrEmpty(disableKeyVault) && disableKeyVault.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return configuration;
        }

        var config = configuration.Build();
        var keyVaultUri = config["Azure:KeyVaultUri"] ?? "https://kv-poshared.vault.azure.net/";

        // Use DefaultAzureCredential which supports Managed Identity in Azure and developer credentials (Azure CLI, Visual Studio) locally.
        // This avoids failing in containers where Azure CLI is not available.
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            // Allow CLI in development machines; DefaultAzureCredential will attempt CLI/VS credentials when available.
            ExcludeAzureCliCredential = false
        });

        configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
        return configuration;
    }

    /// <summary>
    /// Adds Azure OpenAI services for joke analysis.
    /// </summary>
    public static IServiceCollection AddPoJokerAzureOpenAI(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var openAiEndpoint = configuration["Azure:OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("Azure:OpenAI:Endpoint configuration is required. Ensure Key Vault is accessible.");
        var deploymentName = configuration["Azure:OpenAI:DeploymentName"] ?? "gpt-4o-mini";
        var apiKey = configuration["Azure:OpenAI:ApiKey"];

        services.AddSingleton(sp =>
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Use API key authentication — required when the Azure OpenAI resource uses a
                // regional endpoint (e.g. eastus.api.cognitive.microsoft.com) instead of a custom
                // subdomain. Token/Managed-Identity auth only works with custom subdomains.
                return new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(apiKey));
            }

            // Fall back to DefaultAzureCredential (Managed Identity / CLI) when the resource has
            // a custom subdomain that supports token authentication.
            var aiCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeAzureCliCredential = false
            });
            return new AzureOpenAIClient(new Uri(openAiEndpoint), aiCredential);
        });

        services.AddSingleton(new AiJesterSettings { DeploymentName = deploymentName });
        services.AddScoped<IAnalysisService, AiJesterService>();

        return services;
    }
}
