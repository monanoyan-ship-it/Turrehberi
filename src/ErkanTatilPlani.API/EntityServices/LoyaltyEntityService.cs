using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;

namespace ErkanTatilPlani.API.EntityServices;

public class LoyaltyEntityService : ILoyaltyEntityService
{
    private readonly AppDbContext _context;

    public LoyaltyEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TourCredit> GetByVisitorId(int visitorId)
        => _context.TourCredits.Where(c => c.VisitorId == visitorId && c.IsActive);

    public IQueryable<TourCredit> GetExpirableCredits(DateTime before)
        => _context.TourCredits.Where(c =>
            c.IsActive &&
            c.TransactionTypeId == CreditTransactionTypes.Ids.Earn &&
            c.ExpiresAt != null &&
            c.ExpiresAt <= before);

    public void AddCredit(TourCredit credit) => _context.TourCredits.Add(credit);

    public IQueryable<LoyaltyTierHistory> GetTierHistoryByVisitorId(int visitorId)
        => _context.LoyaltyTierHistories.Where(h => h.VisitorId == visitorId && h.IsActive);

    public void AddTierHistory(LoyaltyTierHistory history) => _context.LoyaltyTierHistories.Add(history);
}
