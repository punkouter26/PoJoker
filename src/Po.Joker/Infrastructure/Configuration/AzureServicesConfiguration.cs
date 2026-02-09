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

        services.AddSingleton(sp =>
        {
            // Use DefaultAzureCredential so production uses Managed Identity and developers can use CLI/VS creds.
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
