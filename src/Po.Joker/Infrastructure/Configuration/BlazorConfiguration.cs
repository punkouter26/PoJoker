using Po.Joker.Services;
using Po.Joker.Contracts;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring Blazor components and services.
/// </summary>
public static class BlazorConfiguration
{
    /// <summary>
    /// Adds Blazor components with interactive render modes.
    /// </summary>
    public static IServiceCollection AddPoJokerBlazor(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add HttpClient for Blazor components (server-side)
        services.AddScoped(sp =>
        {
            var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
            return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
        });

        // Add Audio and Speech services (use real client services that work via JS interop over SignalR)
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<ISpeechService, SpeechService>();

        return services;
    }
}
