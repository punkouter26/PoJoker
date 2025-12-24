using Azure.Data.Tables;
using Azure.Identity;

namespace Po.Joker.Infrastructure.Storage;

/// <summary>
/// Configuration helper for Azure Table Storage connections.
/// Uses Aspire-provided TableServiceClient from the AppHost.
/// </summary>
public static class StorageConfiguration
{
    public const string TableName = "jokeperformances";

    /// <summary>
    /// Configures TableClient using Aspire-provided TableServiceClient.
    /// The TableServiceClient is injected via builder.AddAzureTableClient() in Program.cs.
    /// </summary>
    public static IServiceCollection AddPoJokerTableStorage(this IServiceCollection services)
    {
        services.AddSingleton<TableClient>(sp =>
        {
            var tableServiceClient = sp.GetRequiredService<TableServiceClient>();
            var tableClient = tableServiceClient.GetTableClient(TableName);

            // Ensure table exists (fire-and-forget in background)
            _ = Task.Run(async () =>
            {
                try
                {
                    await tableClient.CreateIfNotExistsAsync();
                }
                catch (Exception ex)
                {
                    var logger = sp.GetRequiredService<ILogger<TableClient>>();
                    logger.LogWarning(ex, "Failed to create table {TableName} on startup", TableName);
                }
            });

            return tableClient;
        });

        return services;
    }

    /// <summary>
    /// Legacy configuration for non-Aspire scenarios (standalone deployment).
    /// </summary>
    public static IServiceCollection AddPoJokerStorageLegacy(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<TableServiceClient>(sp =>
        {
            if (environment.IsDevelopment())
            {
                // Use Azurite for local development
                return new TableServiceClient("UseDevelopmentStorage=true");
            }

            // Use Azure Storage with Managed Identity for production
            var storageAccountName = configuration["Azure:StorageAccountName"];
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new InvalidOperationException(
                    "Azure:StorageAccountName must be configured for production environment");
            }

            var tableUri = new Uri($"https://{storageAccountName}.table.core.windows.net");
            return new TableServiceClient(tableUri, new DefaultAzureCredential());
        });

        return services.AddPoJokerTableStorage();
    }

    /// <summary>
    /// Generates an inverted timestamp for RowKey to ensure descending sort order.
    /// </summary>
    public static string GenerateInvertedTimestamp(DateTimeOffset timestamp)
    {
        var invertedTicks = DateTimeOffset.MaxValue.Ticks - timestamp.Ticks;
        return invertedTicks.ToString("D19");
    }

    /// <summary>
    /// Generates a RowKey combining inverted timestamp and performance ID.
    /// </summary>
    public static string GenerateRowKey(DateTimeOffset timestamp, Guid performanceId)
    {
        return $"{GenerateInvertedTimestamp(timestamp)}_{performanceId:N}";
    }
}
