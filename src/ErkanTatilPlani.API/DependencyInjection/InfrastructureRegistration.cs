using ErkanTatilPlani.API.Infrastructure;
using ErkanTatilPlani.API.Services;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;

namespace ErkanTatilPlani.API.DependencyInjection;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IExternalNotificationService, ExternalNotificationService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        services.AddHttpClient<IWeatherService, WeatherService>();
        return services;
    }
}
