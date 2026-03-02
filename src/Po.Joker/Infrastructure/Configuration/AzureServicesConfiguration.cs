using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Po.Joker.Features.Analysis;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.Contracts;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Key Vault secret manager that loads only PoJoker-prefixed secrets
/// and strips the "PoJoker--" prefix so secrets are accessible under
/// their original config key hierarchy (e.g. "PoJoker--Azure--OpenAI--Endpoint"
/// becomes "Azure:OpenAI:Endpoint" in the app configuration).
/// </summary>
internal sealed class PoJokerKeyVaultSecretManager : KeyVaultSecretManager
{
    private const string Prefix = "PoJoker--";

    public override bool Load(SecretProperties secret)
        => secret.Name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);

    public override string GetKey(KeyVaultSecret secret)
        => secret.Name[Prefix.Length..].Replace("--", ConfigurationPath.KeyDelimiter);
}

/// <summary>
/// Extension methods for configuring Azure services.
/// </summary>
public static class AzureServicesConfiguration
{
    /// <summary>
    /// Adds Azure Key Vault configuration using the PoJoker prefix.
    /// Only secrets named "PoJoker--*" are loaded; the prefix is stripped so
    /// existing config keys (Azure:OpenAI:Endpoint, etc.) resolve transparently.
    /// In Development, failure is non-fatal: a warning is printed and the app continues with local config only.
    /// Run 'az login' to enable Key Vault (and real AI) in Development.
    /// </summary>
    public static IConfigurationBuilder AddPoJokerKeyVault(
        this IConfigurationBuilder configuration,
        IHostEnvironment environment)
    {
        // Allow disabling Key Vault via env var or appsettings
        var disableKeyVault = Environment.GetEnvironmentVariable("POJOKER_DISABLE_KEYVAULT")
            ?? configuration.Build()["POJOKER_DISABLE_KEYVAULT"];
        if (!string.IsNullOrEmpty(disableKeyVault) && disableKeyVault.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("[PoJoker] Key Vault disabled via POJOKER_DISABLE_KEYVAULT.");
            return configuration;
        }

        var config = configuration.Build();
        var keyVaultUri = config["Azure:KeyVaultUri"] ?? "https://kv-poshared.vault.azure.net/";

        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeAzureCliCredential = false
        });

        try
        {
            configuration.AddAzureKeyVault(
                new SecretClient(new Uri(keyVaultUri), credential),
                new PoJokerKeyVaultSecretManager());
            Console.WriteLine($"[PoJoker] Key Vault connected: {keyVaultUri} (prefix: PoJoker--)");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  ⚠  KEY VAULT UNAVAILABLE — running with local config only  ║");
            Console.WriteLine("║     Azure OpenAI will use mock AI (no real punchlines)       ║");
            Console.WriteLine("║     To enable real AI: run 'az login' then restart           ║");
            Console.WriteLine($"║     Reason: {ex.Message.Truncate(50),-50} ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        return configuration;
    }

    private static string Truncate(this string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "…";

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

        Console.WriteLine($"[PoJoker] Azure OpenAI endpoint: {openAiEndpoint}");
        Console.WriteLine($"[PoJoker] Azure OpenAI deployment: {deploymentName}");
        Console.WriteLine($"[PoJoker] Azure OpenAI auth: {(string.IsNullOrEmpty(apiKey) ? "DefaultAzureCredential" : "ApiKey")}");

        // Use the latest stable API version that supports gpt-4.1-nano
        var clientOptions = new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_10_21);

        services.AddSingleton(sp =>
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Use API key authentication — required when the Azure OpenAI resource uses a
                // regional endpoint (e.g. eastus.api.cognitive.microsoft.com) instead of a custom
                // subdomain. Token/Managed-Identity auth only works with custom subdomains.
                return new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(apiKey), clientOptions);
            }

            // Fall back to DefaultAzureCredential (Managed Identity / CLI) when the resource has
            // a custom subdomain that supports token authentication.
            var aiCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeAzureCliCredential = false
            });
            return new AzureOpenAIClient(new Uri(openAiEndpoint), aiCredential, clientOptions);
        });

        services.AddSingleton(new AiJesterSettings { DeploymentName = deploymentName });
        services.AddScoped<IAnalysisService, AiJesterService>();

        return services;
    }
}
