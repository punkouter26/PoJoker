using Scalar.AspNetCore;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring API documentation using Microsoft.AspNetCore.OpenApi.
/// Replaces Swashbuckle with the native OpenAPI support introduced in .NET 9+.
/// Uses Scalar as a lightweight, modern API reference UI.
/// </summary>
public static class ApiDocumentationConfiguration
{
    /// <summary>
    /// Registers the built-in OpenAPI document generation services.
    /// </summary>
    public static IServiceCollection AddPoJokerApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title = "The Digital Jester API";
                document.Info.Version = "v1";
                document.Info.Description = "A Medieval AI Comedy Experience - API endpoints for joke fetching and analysis.";
                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Maps the OpenAPI JSON endpoint and the Scalar API reference UI.
    /// Scalar UI is only exposed in Development.
    /// </summary>
    public static WebApplication UsePoJokerOpenApi(this WebApplication app)
    {
        // Always expose the OpenAPI JSON document
        app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            // Scalar provides a modern, interactive API reference UI
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("The Digital Jester - API Documentation");
            });
        }

        return app;
    }
}
