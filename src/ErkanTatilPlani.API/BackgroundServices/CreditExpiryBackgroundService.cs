using ErkanTatilPlani.Core.Factories.Loyalty;

namespace ErkanTatilPlani.API.BackgroundServices;

public class CreditExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CreditExpiryBackgroundService> _logger;

    public CreditExpiryBackgroundService(IServiceProvider serviceProvider, ILogger<CreditExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var loyaltyFactory = scope.ServiceProvider.GetRequiredService<ILoyaltyFactory>();
                await loyaltyFactory.ExpireCreditsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Credit expiry background service error");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
