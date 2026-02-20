using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class NotificationEntityService : INotificationEntityService
{
    private readonly AppDbContext _context;

    public NotificationEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Notification> GetByVisitorId(int visitorId)
        => _context.Notifications.Where(n => n.VisitorId == visitorId && n.IsActive);

    public async Task<int> GetUnreadCountAsync(int visitorId)
        => await _context.Notifications.CountAsync(n => n.VisitorId == visitorId && n.IsActive && !n.IsRead);

    public async Task<Notification?> GetByIdAsync(int id)
        => await _context.Notifications.FindAsync(id);

    public void Add(Notification notification) => _context.Notifications.Add(notification);
}
