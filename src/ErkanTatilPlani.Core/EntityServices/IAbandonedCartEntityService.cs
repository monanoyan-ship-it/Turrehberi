using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IAbandonedCartEntityService
{
    IQueryable<AbandonedCart> GetUnrecoveredCarts(DateTime olderThan);
    Task<AbandonedCart?> GetByEmailAndTourAsync(string email, int tourId);
    Task<AbandonedCart?> GetByVisitorAndTourAsync(int visitorId, int tourId);
    void Add(AbandonedCart cart);
    void Update(AbandonedCart cart);
}
