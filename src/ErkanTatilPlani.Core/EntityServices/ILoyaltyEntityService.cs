using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ILoyaltyEntityService
{
    IQueryable<TourCredit> GetByVisitorId(int visitorId);
    IQueryable<TourCredit> GetExpirableCredits(DateTime before);
    void AddCredit(TourCredit credit);

    IQueryable<LoyaltyTierHistory> GetTierHistoryByVisitorId(int visitorId);
    void AddTierHistory(LoyaltyTierHistory history);
}
