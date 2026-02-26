using ErkanTatilPlani.Core.Factories.AbandonedCarts;
using ErkanTatilPlani.Core.Factories.ScheduledEmails;

namespace ErkanTatilPlani.API.BackgroundServices;

public class ScheduledEmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledEmailBackgroundService> _logger;

    public ScheduledEmailBackgroundService(IServiceProvider serviceProvider, ILogger<ScheduledEmailBackgroundService> logger)
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

                var emailFactory = scope.ServiceProvider.GetRequiredService<IScheduledEmailFactory>();
                await emailFactory.ProcessPendingEmailsAsync();

                var cartFactory = scope.ServiceProvider.GetRequiredService<IAbandonedCartFactory>();
                await cartFactory.ProcessAbandonedCartsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled email background service error");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
