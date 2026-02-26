using System.Text.Json;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Notifications;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Notifications;

public class NotificationFactory : INotificationFactory
{
    private readonly INotificationEntityService _notificationService;
    private readonly ITourWatchEntityService _watchService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationFactory(
        INotificationEntityService notificationService,
        ITourWatchEntityService watchService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _watchService = watchService;
        _tourService = tourService;
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetNotificationsAsync(int visitorId, int page, int pageSize)
    {
        var query = _notificationService.GetByVisitorId(visitorId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.TitleKey,
                n.MessageKey,
                n.MessageParams,
                n.NotificationTypeId,
                NotificationType = NotificationTypes.GetById(n.NotificationTypeId)!.SystemName,
                NotificationIcon = NotificationTypes.GetById(n.NotificationTypeId)!.Icon,
                NotificationCss = NotificationTypes.GetById(n.NotificationTypeId)!.CssClass,
                n.RelatedEntityType,
                n.RelatedEntityId,
                n.IsRead,
                n.ReadAt,
                n.CreatedAt
            })
            .ToListAsync();

        return new
        {
            notifications,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<int> GetUnreadCountAsync(int visitorId)
        => await _notificationService.GetUnreadCountAsync(visitorId);

    public async Task<(bool success, string message)> MarkAsReadAsync(int visitorId, int notificationId)
    {
        var notification = await _notificationService.GetByIdAsync(notificationId);
        if (notification == null || notification.VisitorId != visitorId)
            return (false, "Error.NotificationNotFound");

        if (notification.IsRead)
            return (true, "Error.NotificationAlreadyRead");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.NotificationMarkedAsRead");
    }

    public async Task<(bool success, string message)> MarkAllAsReadAsync(int visitorId)
    {
        var unreadNotifications = await _notificationService.GetByVisitorId(visitorId)
            .Where(n => !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.AllNotificationsMarkedAsRead");
    }

    public async Task CreateScarcityNotificationsAsync(int tourId, int remainingSlots)
    {
        var watchers = await _watchService.GetActiveWatchersForTourAsync(tourId);
        var scarcityWatchers = watchers.Where(w => w.NotifyScarcity).ToList();

        if (!scarcityWatchers.Any())
            return;

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return;

        var messageParams = JsonSerializer.Serialize(new { tourName = tour.Name, remaining = remainingSlots });

        foreach (var watcher in scarcityWatchers)
        {
            _notificationService.Add(new Notification
            {
                VisitorId = watcher.VisitorId,
                TitleKey = "Notification.Scarcity.Title",
                MessageKey = "Notification.Scarcity.Message",
                MessageParams = messageParams,
                NotificationTypeId = NotificationTypes.Ids.Scarcity,
                RelatedEntityType = "Tour",
                RelatedEntityId = tourId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CreatePriceChangeNotificationAsync(int tourId, decimal oldPrice, decimal newPrice)
    {
        var watchers = await _watchService.GetActiveWatchersForTourAsync(tourId);
        var priceWatchers = watchers.Where(w => w.NotifyPriceChange).ToList();

        if (!priceWatchers.Any())
            return;

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return;

        var messageParams = JsonSerializer.Serialize(new { tourName = tour.Name, oldPrice, newPrice });

        foreach (var watcher in priceWatchers)
        {
            _notificationService.Add(new Notification
            {
                VisitorId = watcher.VisitorId,
                TitleKey = "Notification.PriceChange.Title",
                MessageKey = "Notification.PriceChange.Message",
                MessageParams = messageParams,
                NotificationTypeId = NotificationTypes.Ids.PriceChange,
                RelatedEntityType = "Tour",
                RelatedEntityId = tourId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CreateReservationNotificationAsync(int visitorId, int reservationId, string type)
    {
        var titleKey = type switch
        {
            "confirmed" => "Notification.Reservation.Confirmed.Title",
            "cancelled" => "Notification.Reservation.Cancelled.Title",
            "completed" => "Notification.Reservation.Completed.Title",
            _ => "Notification.Reservation.Title"
        };

        var messageKey = type switch
        {
            "confirmed" => "Notification.Reservation.Confirmed.Message",
            "cancelled" => "Notification.Reservation.Cancelled.Message",
            "completed" => "Notification.Reservation.Completed.Message",
            _ => "Notification.Reservation.Message"
        };

        _notificationService.Add(new Notification
        {
            VisitorId = visitorId,
            TitleKey = titleKey,
            MessageKey = messageKey,
            MessageParams = JsonSerializer.Serialize(new { reservationId }),
            NotificationTypeId = NotificationTypes.Ids.Reservation,
            RelatedEntityType = "Reservation",
            RelatedEntityId = reservationId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CreateSocialNotificationAsync(int targetVisitorId, int actorVisitorId, int notificationTypeId, string titleKey, string messageKey, string relatedEntityType, int relatedEntityId)
    {
        var actor = await _visitorService.GetByIdAsync(actorVisitorId);
        var actorName = actor != null ? $"{actor.FirstName} {actor.LastName}" : "?";

        _notificationService.Add(new Notification
        {
            VisitorId = targetVisitorId,
            TitleKey = titleKey,
            MessageKey = messageKey,
            MessageParams = JsonSerializer.Serialize(new { actorName, actorId = actorVisitorId }),
            NotificationTypeId = notificationTypeId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
    }
}
