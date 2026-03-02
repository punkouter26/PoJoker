using Po.Joker.Services;
using Po.Joker.Contracts;
using Po.Joker.Application;
using Microsoft.Extensions.Options;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring Blazor components and services.
/// </summary>
public static class BlazorConfiguration
{
    /// <summary>
    /// Adds Blazor components with interactive render modes.
    /// </summary>
    public static IServiceCollection AddPoJokerBlazor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add HttpClient for Blazor components (server-side)
        services.AddScoped(sp =>
        {
            var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
            return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
        });

        // Add Audio and Speech services
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<ISpeechService, SpeechService>();

        // Configure performance settings from appsettings.json with override support
        services.Configure<PerformanceSettings>(options =>
        {
            var performanceSection = configuration.GetSection("Jester:Performance");
            if (performanceSection.Exists())
            {
                performanceSection.Bind(options);
            }

            // Allow environment variable overrides
            if (int.TryParse(Environment.GetEnvironmentVariable("POJOKER_SETUP_DURATION_SECONDS"), out int setupDuration))
                options.SetupDurationSeconds = setupDuration;
            if (int.TryParse(Environment.GetEnvironmentVariable("POJOKER_PREDICTION_DURATION_SECONDS"), out int predDuration))
                options.PredictionDurationSeconds = predDuration;
            if (int.TryParse(Environment.GetEnvironmentVariable("POJOKER_PUNCHLINE_DELAY_SECONDS"), out int punchlineDelay))
                options.PunchlineDelaySeconds = punchlineDelay;
            if (int.TryParse(Environment.GetEnvironmentVariable("POJOKER_PUNCHLINE_DURATION_SECONDS"), out int punchlineDuration))
                options.PunchlineDurationSeconds = punchlineDuration;
            if (int.TryParse(Environment.GetEnvironmentVariable("POJOKER_TRANSITION_DURATION_SECONDS"), out int transitionDuration))
                options.TransitionDurationSeconds = transitionDuration;

            options.Validate();
        });

        return services;
    }
}
