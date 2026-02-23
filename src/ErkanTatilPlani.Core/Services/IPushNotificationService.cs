namespace ErkanTatilPlani.Core.Services;

public interface IPushNotificationService
{
    Task<bool> SendPushNotificationAsync(int visitorId, string title, string message, string? url = null);
    Task<bool> SendPushNotificationToAllAsync(string title, string message, string? url = null);
}
