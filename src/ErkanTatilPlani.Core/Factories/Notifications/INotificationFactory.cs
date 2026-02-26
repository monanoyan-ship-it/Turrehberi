namespace ErkanTatilPlani.Core.Factories.Notifications;

public interface INotificationFactory
{
    Task<object> GetNotificationsAsync(int visitorId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(int visitorId);
    Task<(bool success, string message)> MarkAsReadAsync(int visitorId, int notificationId);
    Task<(bool success, string message)> MarkAllAsReadAsync(int visitorId);
    Task CreateScarcityNotificationsAsync(int tourId, int remainingSlots);
    Task CreatePriceChangeNotificationAsync(int tourId, decimal oldPrice, decimal newPrice);
    Task CreateReservationNotificationAsync(int visitorId, int reservationId, string type);
    Task CreateSocialNotificationAsync(int targetVisitorId, int actorVisitorId, int notificationTypeId, string titleKey, string messageKey, string relatedEntityType, int relatedEntityId);
}
