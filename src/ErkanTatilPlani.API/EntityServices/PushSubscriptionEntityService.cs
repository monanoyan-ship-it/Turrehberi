using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class PushSubscriptionEntityService : IPushSubscriptionEntityService
{
    private readonly AppDbContext _context;

    public PushSubscriptionEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<PushSubscription> GetByVisitorId(int visitorId)
        => _context.PushSubscriptions.Where(ps => ps.VisitorId == visitorId && ps.IsActive);

    public async Task<PushSubscription?> GetByEndpointAsync(string endpoint)
        => await _context.PushSubscriptions.FirstOrDefaultAsync(ps => ps.Endpoint == endpoint && ps.IsActive);

    public async Task<List<PushSubscription>> GetAllActiveAsync()
        => await _context.PushSubscriptions.Where(ps => ps.IsActive).ToListAsync();

    public void Add(PushSubscription subscription) => _context.PushSubscriptions.Add(subscription);

    public void Remove(PushSubscription subscription) => subscription.IsActive = false;
}
