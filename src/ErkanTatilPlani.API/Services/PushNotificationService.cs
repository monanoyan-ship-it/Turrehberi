using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ErkanTatilPlani.API.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushSubscriptionEntityService _subscriptionService;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IPushSubscriptionEntityService subscriptionService,
        ILogger<PushNotificationService> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task<bool> SendPushNotificationAsync(int visitorId, string title, string message, string? url = null)
    {
        var subscriptions = await _subscriptionService.GetByVisitorId(visitorId)
            .ToListAsync();

        if (!subscriptions.Any())
        {
            _logger.LogInformation("Push notification: No subscriptions for visitor {VisitorId}", visitorId);
            return false;
        }

        foreach (var sub in subscriptions)
        {
            _logger.LogInformation(
                "Push notification stub: Sending to visitor {VisitorId}, endpoint {Endpoint}, title: {Title}, message: {Message}",
                visitorId, sub.Endpoint, title, message);
        }

        return true;
    }

    public async Task<bool> SendPushNotificationToAllAsync(string title, string message, string? url = null)
    {
        var subscriptions = await _subscriptionService.GetAllActiveAsync();

        _logger.LogInformation(
            "Push notification stub: Broadcasting to {Count} subscribers, title: {Title}",
            subscriptions.Count, title);

        return true;
    }
}
