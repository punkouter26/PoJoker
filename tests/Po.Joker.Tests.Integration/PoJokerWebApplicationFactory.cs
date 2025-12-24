using Azure.Data.Tables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Po.Joker.Features.Analysis;
using Po.Joker.Shared.Contracts;

namespace Po.Joker.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures Azurite for storage and mocks AI services.
/// </summary>
public class PoJokerWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Azurite connection string for local development storage.
    /// </summary>
    private const string AzuriteConnectionString = "UseDevelopmentStorage=true";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add connection string for Azurite
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:tables"] = AzuriteConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real AI service and replace with mock
            var analysisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAnalysisService));
            if (analysisDescriptor != null)
            {
                services.Remove(analysisDescriptor);
            }
            services.AddSingleton<IAnalysisService, MockAnalysisService>();

            // Remove the Aspire-configured TableServiceClient and use Azurite directly
            var tableServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TableServiceClient));
            if (tableServiceDescriptor != null)
            {
                services.Remove(tableServiceDescriptor);
            }

            // Add TableServiceClient pointing to Azurite
            services.AddSingleton<TableServiceClient>(_ => 
                new TableServiceClient(AzuriteConnectionString));
        });
    }
}
