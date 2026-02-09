using FluentValidation;
using MediatR;
using Po.Joker.Validation;

namespace Po.Joker.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring MediatR and validation.
/// </summary>
public static class MediatRConfiguration
{
    /// <summary>
    /// Adds MediatR for CQRS pattern and FluentValidation.
    /// </summary>
    public static IServiceCollection AddPoJokerMediatR(this IServiceCollection services)
    {
        // Add MediatR for CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        // Add FluentValidation
        services.AddValidatorsFromAssemblyContaining<JokeDtoValidator>();

        return services;
    }
}
