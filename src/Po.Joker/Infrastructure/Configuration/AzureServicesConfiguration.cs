using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Po.Joker.Features.Analysis;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Shared.Contracts;

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
        var config = configuration.Build();
        var keyVaultUri = config["Azure:KeyVaultUri"] ?? "https://pojoker-kv.vault.azure.net/";
        var credential = environment.IsDevelopment()
            ? new AzureCliCredential()
            : (TokenCredential)new DefaultAzureCredential();

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
            var aiCredential = environment.IsDevelopment()
                ? new AzureCliCredential()
                : (TokenCredential)new DefaultAzureCredential();
            return new AzureOpenAIClient(new Uri(openAiEndpoint), aiCredential);
        });

        services.AddSingleton(new AiJesterSettings { DeploymentName = deploymentName });
        services.AddScoped<IAnalysisService, AiJesterService>();

        return services;
    }
}
