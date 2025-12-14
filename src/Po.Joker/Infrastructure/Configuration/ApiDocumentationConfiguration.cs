using Swashbuckle.AspNetCore.SwaggerGen;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring API documentation.
/// </summary>
public static class ApiDocumentationConfiguration
{
    /// <summary>
    /// Adds Swagger/OpenAPI documentation.
    /// </summary>
    public static IServiceCollection AddPoJokerApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "The Digital Jester API",
                Version = "v1",
                Description = "A Medieval AI Comedy Experience - API endpoints for joke fetching and analysis."
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware.
    /// </summary>
    public static IApplicationBuilder UsePoJokerSwagger(this IApplicationBuilder app, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Jester API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "The Digital Jester - API Documentation";
            });
        }

        return app;
    }
}
