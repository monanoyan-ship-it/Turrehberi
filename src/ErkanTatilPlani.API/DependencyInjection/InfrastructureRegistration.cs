using ErkanTatilPlani.API.Infrastructure;
using ErkanTatilPlani.Core.Infrastructure;

namespace ErkanTatilPlani.API.DependencyInjection;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }
}
