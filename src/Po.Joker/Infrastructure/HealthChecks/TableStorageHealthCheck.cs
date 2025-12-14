using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Po.Joker.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Azure Table Storage connectivity.
/// </summary>
public sealed class TableStorageHealthCheck : IHealthCheck
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<TableStorageHealthCheck> _logger;

    public TableStorageHealthCheck(
        TableServiceClient tableServiceClient,
        ILogger<TableStorageHealthCheck> logger)
    {
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // List tables to verify connectivity
            await foreach (var table in _tableServiceClient.QueryAsync(cancellationToken: cancellationToken))
            {
                // Just checking connectivity, break after first result
                break;
            }

            return HealthCheckResult.Healthy("Azure Table Storage is accessible.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Table Storage health check failed");
            return HealthCheckResult.Unhealthy("Unable to reach Azure Table Storage.", ex);
        }
    }
}
