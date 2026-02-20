using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class PromotionEntityService : IPromotionEntityService
{
    private readonly AppDbContext _context;

    public PromotionEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Promotion> GetByCompanyId(int companyId)
        => _context.Promotions.Where(p => p.CompanyId == companyId && p.IsActive);

    public IQueryable<Promotion> GetActiveByCompanyId(int companyId)
        => _context.Promotions.Where(p => p.CompanyId == companyId && p.IsActive
            && p.StatusId == PromotionStatuses.Ids.Active
            && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

    public IQueryable<Promotion> GetActiveByType(int companyId, int promotionTypeId)
        => GetActiveByCompanyId(companyId).Where(p => p.PromotionTypeId == promotionTypeId);

    public async Task<Promotion?> GetByIdAsync(int id)
        => await _context.Promotions.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Promotion?> GetByCodeAsync(int companyId, string code)
        => await _context.Promotions.FirstOrDefaultAsync(p =>
            p.CompanyId == companyId && p.Code == code && p.IsActive
            && p.StatusId == PromotionStatuses.Ids.Active
            && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

    public IQueryable<Promotion> GetActiveFlashSales()
        => _context.Promotions.Include(p => p.Company)
            .Where(p => p.IsActive && p.StatusId == PromotionStatuses.Ids.Active
                && p.PromotionTypeId == PromotionTypes.Ids.FlashSale
                && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow
                && (p.FlashSaleStock == null || p.FlashSaleSoldCount < p.FlashSaleStock));

    public IQueryable<Promotion> GetActiveLastMinuteDeals()
        => _context.Promotions.Include(p => p.Company)
            .Where(p => p.IsActive && p.StatusId == PromotionStatuses.Ids.Active
                && p.PromotionTypeId == PromotionTypes.Ids.LastMinute
                && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

    public IQueryable<Promotion> GetAllActive()
        => _context.Promotions.Where(p => p.IsActive
            && p.StatusId == PromotionStatuses.Ids.Active
            && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

    public void Add(Promotion promotion) => _context.Promotions.Add(promotion);

    public void Update(Promotion promotion) => _context.Entry(promotion).State = EntityState.Modified;

    // Usage tracking
    public async Task<int> GetUsageCountByVisitorAsync(int promotionId, int visitorId)
        => await _context.PromotionUsages.CountAsync(u => u.PromotionId == promotionId && u.VisitorId == visitorId);

    public IQueryable<PromotionUsage> GetUsagesByReservation(int reservationId)
        => _context.PromotionUsages.Where(u => u.ReservationId == reservationId);

    public void AddUsage(PromotionUsage usage) => _context.PromotionUsages.Add(usage);
}
