using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IPromotionEntityService
{
    IQueryable<Promotion> GetByCompanyId(int companyId);
    IQueryable<Promotion> GetActiveByCompanyId(int companyId);
    IQueryable<Promotion> GetActiveByType(int companyId, int promotionTypeId);
    Task<Promotion?> GetByIdAsync(int id);
    Task<Promotion?> GetByCodeAsync(int companyId, string code);
    IQueryable<Promotion> GetActiveFlashSales();
    IQueryable<Promotion> GetActiveLastMinuteDeals();
    IQueryable<Promotion> GetAllActive();
    void Add(Promotion promotion);
    void Update(Promotion promotion);

    // Usage tracking
    Task<int> GetUsageCountByVisitorAsync(int promotionId, int visitorId);
    IQueryable<PromotionUsage> GetUsagesByReservation(int reservationId);
    void AddUsage(PromotionUsage usage);
}
