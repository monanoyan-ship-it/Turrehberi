using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface INotificationEntityService
{
    IQueryable<Notification> GetByVisitorId(int visitorId);
    Task<int> GetUnreadCountAsync(int visitorId);
    Task<Notification?> GetByIdAsync(int id);
    void Add(Notification notification);
}
