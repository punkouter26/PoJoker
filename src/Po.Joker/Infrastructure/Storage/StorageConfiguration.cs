using Azure.Data.Tables;
using Azure.Identity;

namespace Po.Joker.Infrastructure.Storage;

/// <summary>
/// Configuration for Azure Table Storage connections.
/// Uses Azurite (UseDevelopmentStorage) locally and Managed Identity in production.
/// </summary>
public static class StorageConfiguration
{
    public const string TableName = "jokeperformances";

    /// <summary>
    /// Registers TableServiceClient and TableClient for Azure Table Storage.
    /// In Development: uses connection string from config or Azurite fallback.
    /// In Production: uses Managed Identity with the configured storage account name.
    /// </summary>
    public static IServiceCollection AddPoJokerStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<TableServiceClient>(sp =>
        {
            // Check for explicit connection string first (integration tests, CI, Azurite)
            var connectionString = configuration.GetConnectionString("tables");
            if (!string.IsNullOrEmpty(connectionString))
            {
                return new TableServiceClient(connectionString);
            }

            if (environment.IsDevelopment())
            {
                // Default to Azurite for local development
                return new TableServiceClient("UseDevelopmentStorage=true");
            }

            // Production: use Managed Identity with storage account name
            var storageAccountName = configuration["Azure:StorageAccountName"];
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new InvalidOperationException(
                    "Azure:StorageAccountName must be configured for production, or provide ConnectionStrings:tables.");
            }

            var tableUri = new Uri($"https://{storageAccountName}.table.core.windows.net");
            return new TableServiceClient(tableUri, new DefaultAzureCredential());
        });

        // Register the named TableClient for the jokeperformances table
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
