using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IPushSubscriptionEntityService
{
    IQueryable<PushSubscription> GetByVisitorId(int visitorId);
    Task<PushSubscription?> GetByEndpointAsync(string endpoint);
    Task<List<PushSubscription>> GetAllActiveAsync();
    void Add(PushSubscription subscription);
    void Remove(PushSubscription subscription);
}
