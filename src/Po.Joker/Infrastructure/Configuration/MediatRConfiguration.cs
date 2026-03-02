using MediatR;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring MediatR.
/// </summary>
public static class MediatRConfiguration
{
    /// <summary>
    /// Adds MediatR for CQRS pattern.
    /// </summary>
    public static IServiceCollection AddPoJokerMediatR(this IServiceCollection services)
    {
        // Add MediatR for CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        return services;
    }
}
