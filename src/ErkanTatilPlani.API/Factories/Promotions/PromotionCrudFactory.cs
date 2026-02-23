using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Promotions;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Promotions;

public class PromotionCrudFactory : IPromotionCrudFactory
{
    private readonly IPromotionEntityService _promotionService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ICompanyEntityService _companyService;
    private readonly IUnitOfWork _unitOfWork;

    public PromotionCrudFactory(
        IPromotionEntityService promotionService,
        IVisitorEntityService visitorService,
        ICompanyEntityService companyService,
        IUnitOfWork unitOfWork)
    {
        _promotionService = promotionService;
        _visitorService = visitorService;
        _companyService = companyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object result, int statusCode)> GetCompanyPromotionsAsync(
        int visitorId, int? typeFilter = null, int? statusFilter = null)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.CompanyId == null)
            return (false, new { message = "Firma bulunamadi" }, 403);

        var query = _promotionService.GetByCompanyId(visitor.CompanyId.Value);

        if (typeFilter.HasValue)
            query = query.Where(p => p.PromotionTypeId == typeFilter.Value);
        if (statusFilter.HasValue)
            query = query.Where(p => p.StatusId == statusFilter.Value);

        var promotions = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var result = promotions.Select(p => MapPromotionToDto(p));
        return (true, result, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetPromotionByIdAsync(
        int visitorId, int promotionId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.CompanyId == null)
            return (false, new { message = "Firma bulunamadi" }, 403);

        var promotion = await _promotionService.GetByIdAsync(promotionId);
        if (promotion == null || !promotion.IsActive)
            return (false, new { message = "Promosyon bulunamadi" }, 404);

        if (promotion.CompanyId != visitor.CompanyId.Value)
            return (false, new { message = "Bu promosyona erisim yetkiniz yok" }, 403);

        return (true, MapPromotionToDto(promotion), 200);
    }

    public async Task<(bool success, object result, int statusCode)> CreatePromotionAsync(
        int visitorId, Promotion promotion)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.CompanyId == null)
            return (false, new { message = "Firma bulunamadi" }, 403);

        var company = await _companyService.GetByIdAsync(visitor.CompanyId.Value);
        if (company == null || company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, new { message = "Firmaniz onaylanmamis" }, 403);

        // Kupon kodu unique kontrolu
        if (promotion.PromotionTypeId == PromotionTypes.Ids.Coupon && !string.IsNullOrEmpty(promotion.Code))
        {
            var existing = await _promotionService.GetByCodeAsync(visitor.CompanyId.Value, promotion.Code);
            if (existing != null)
                return (false, new { message = "Bu kupon kodu zaten kullaniliyor" }, 400);
        }

        promotion.CompanyId = visitor.CompanyId.Value;
        promotion.CreatedByVisitorId = visitorId;
        promotion.StatusId = PromotionStatuses.Ids.Active;
        promotion.StartDate = DateTime.SpecifyKind(promotion.StartDate, DateTimeKind.Utc);
        promotion.EndDate = DateTime.SpecifyKind(promotion.EndDate, DateTimeKind.Utc);

        _promotionService.Add(promotion);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Promosyon olusturuldu", id = promotion.Id }, 201);
    }

    public async Task<(bool success, object result, int statusCode)> UpdatePromotionAsync(
        int visitorId, int promotionId, Promotion updated)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.CompanyId == null)
            return (false, new { message = "Firma bulunamadi" }, 403);

        var promotion = await _promotionService.GetByIdAsync(promotionId);
        if (promotion == null || !promotion.IsActive)
            return (false, new { message = "Promosyon bulunamadi" }, 404);

        if (promotion.CompanyId != visitor.CompanyId.Value)
            return (false, new { message = "Bu promosyona erisim yetkiniz yok" }, 403);

        // Kupon kodu unique kontrolu
        if (updated.PromotionTypeId == PromotionTypes.Ids.Coupon && !string.IsNullOrEmpty(updated.Code))
        {
            var existing = await _promotionService.GetByCodeAsync(visitor.CompanyId.Value, updated.Code);
            if (existing != null && existing.Id != promotionId)
                return (false, new { message = "Bu kupon kodu zaten kullaniliyor" }, 400);
        }

        // Guncelle
        promotion.Name = updated.Name;
        promotion.Description = updated.Description;
        promotion.PromotionTypeId = updated.PromotionTypeId;
        promotion.DiscountTypeId = updated.DiscountTypeId;
        promotion.DiscountValue = updated.DiscountValue;
        promotion.StartDate = DateTime.SpecifyKind(updated.StartDate, DateTimeKind.Utc);
        promotion.EndDate = DateTime.SpecifyKind(updated.EndDate, DateTimeKind.Utc);
        promotion.Code = updated.Code;
        promotion.UsageLimit = updated.UsageLimit;
        promotion.UsageLimitPerUser = updated.UsageLimitPerUser;
        promotion.MinOrderAmount = updated.MinOrderAmount;
        promotion.MaxDiscountAmount = updated.MaxDiscountAmount;
        promotion.MinDaysAhead = updated.MinDaysAhead;
        promotion.MaxHoursBeforeStart = updated.MaxHoursBeforeStart;
        promotion.MinAvailableCapacityPercent = updated.MinAvailableCapacityPercent;
        promotion.MinGroupSize = updated.MinGroupSize;
        promotion.MaxGroupSize = updated.MaxGroupSize;
        promotion.FlashSaleStock = updated.FlashSaleStock;
        promotion.BundleTourIds = updated.BundleTourIds;
        promotion.MinBundleTourCount = updated.MinBundleTourCount;
        promotion.SeasonRules = updated.SeasonRules;
        promotion.WeekendMultiplier = updated.WeekendMultiplier;
        promotion.HighDemandMultiplier = updated.HighDemandMultiplier;
        promotion.HighDemandThresholdPercent = updated.HighDemandThresholdPercent;
        promotion.ApplicableTourIds = updated.ApplicableTourIds;
        promotion.UpdatedAt = DateTime.UtcNow;

        _promotionService.Update(promotion);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Promosyon guncellendi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeletePromotionAsync(
        int visitorId, int promotionId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.CompanyId == null)
            return (false, new { message = "Firma bulunamadi" }, 403);

        var promotion = await _promotionService.GetByIdAsync(promotionId);
        if (promotion == null || !promotion.IsActive)
            return (false, new { message = "Promosyon bulunamadi" }, 404);

        if (promotion.CompanyId != visitor.CompanyId.Value)
            return (false, new { message = "Bu promosyona erisim yetkiniz yok" }, 403);

        promotion.IsActive = false;
        promotion.UpdatedAt = DateTime.UtcNow;
        _promotionService.Update(promotion);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Promosyon silindi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> ToggleStatusAsync(
        int visitorId, int promotionId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.CompanyId == null)
            return (false, new { message = "Firma bulunamadi" }, 403);

        var promotion = await _promotionService.GetByIdAsync(promotionId);
        if (promotion == null || !promotion.IsActive)
            return (false, new { message = "Promosyon bulunamadi" }, 404);

        if (promotion.CompanyId != visitor.CompanyId.Value)
            return (false, new { message = "Bu promosyona erisim yetkiniz yok" }, 403);

        promotion.StatusId = promotion.StatusId == PromotionStatuses.Ids.Active
            ? PromotionStatuses.Ids.Disabled
            : PromotionStatuses.Ids.Active;
        promotion.UpdatedAt = DateTime.UtcNow;
        _promotionService.Update(promotion);
        await _unitOfWork.SaveChangesAsync();

        var statusName = PromotionStatuses.GetById(promotion.StatusId)?.SystemName ?? "Unknown";
        return (true, new { message = $"Promosyon durumu: {statusName}", statusId = promotion.StatusId }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetAllPromotionsAsync(
        int? companyId = null, int? typeFilter = null, int? statusFilter = null)
    {
        var query = _promotionService.GetByCompanyId(0); // Placeholder
        // Admin icin tum promosyonlari getir
        query = _promotionService.GetAllActive();

        // Tum aktif/pasif/expired promosyonlari getir (admin gorunumu icin IsActive filtresi yeterli)
        if (companyId.HasValue)
            query = query.Where(p => p.CompanyId == companyId.Value);
        if (typeFilter.HasValue)
            query = query.Where(p => p.PromotionTypeId == typeFilter.Value);
        if (statusFilter.HasValue)
            query = query.Where(p => p.StatusId == statusFilter.Value);

        var promotions = await query.Include(p => p.Company)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

        var result = promotions.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.PromotionTypeId,
            promotionType = PromotionTypes.GetById(p.PromotionTypeId)?.SystemName,
            p.DiscountTypeId,
            discountType = DiscountTypes.GetById(p.DiscountTypeId)?.SystemName,
            p.DiscountValue,
            p.StatusId,
            status = PromotionStatuses.GetById(p.StatusId)?.SystemName,
            p.StartDate,
            p.EndDate,
            p.Code,
            p.UsageCount,
            p.UsageLimit,
            companyId = p.CompanyId,
            companyName = p.Company?.Name
        });

        return (true, result, 200);
    }

    private static object MapPromotionToDto(Promotion p) => new
    {
        p.Id,
        p.Name,
        p.Description,
        p.PromotionTypeId,
        promotionType = PromotionTypes.GetById(p.PromotionTypeId)?.SystemName,
        p.DiscountTypeId,
        discountType = DiscountTypes.GetById(p.DiscountTypeId)?.SystemName,
        p.DiscountValue,
        p.StatusId,
        status = PromotionStatuses.GetById(p.StatusId)?.SystemName,
        p.StartDate,
        p.EndDate,
        p.Code,
        p.UsageLimit,
        p.UsageCount,
        p.UsageLimitPerUser,
        p.MinOrderAmount,
        p.MaxDiscountAmount,
        p.MinDaysAhead,
        p.MaxHoursBeforeStart,
        p.MinAvailableCapacityPercent,
        p.MinGroupSize,
        p.MaxGroupSize,
        p.FlashSaleStock,
        p.FlashSaleSoldCount,
        p.BundleTourIds,
        p.MinBundleTourCount,
        p.SeasonRules,
        p.WeekendMultiplier,
        p.HighDemandMultiplier,
        p.HighDemandThresholdPercent,
        p.ApplicableTourIds,
        p.CreatedAt
    };
}
