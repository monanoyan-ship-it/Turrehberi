using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Promotions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ErkanTatilPlani.API.Factories.Promotions;

public class PromotionCalculationFactory : IPromotionCalculationFactory
{
    private readonly IPromotionEntityService _promotionService;
    private readonly ITourEntityService _tourService;
    private const decimal MaxTotalDiscountPercent = 0.50m; // Max %50 indirim

    public PromotionCalculationFactory(
        IPromotionEntityService promotionService,
        ITourEntityService tourService)
    {
        _promotionService = promotionService;
        _tourService = tourService;
    }

    public async Task<PriceCalculationResult> CalculatePriceAsync(
        int tourId, int numberOfPeople, DateTime? tourStartDate = null,
        string? couponCode = null, int? visitorId = null)
    {
        var tour = await _tourService.GetByIdWithCompanyAsync(tourId);
        if (tour == null || tour.Company == null)
            return new PriceCalculationResult();

        var basePrice = tour.Price;
        var subTotal = basePrice * numberOfPeople;
        var currentPrice = subTotal;
        var appliedDiscounts = new List<AppliedDiscount>();
        var startDate = tourStartDate ?? DateTime.UtcNow.AddDays(7);

        // Firma promosyonlarini al
        var promotions = await _promotionService.GetActiveByCompanyId(tour.CompanyId).ToListAsync();

        // Uygulanabilir promosyonlari filtrele (ApplicableTourIds kontrolu)
        promotions = promotions.Where(p => IsApplicableToTour(p, tourId)).ToList();

        // Indirim uygulama sirasi: DynamicPricing -> EarlyBird/LastMinute -> GroupDiscount -> Coupon
        bool hasFlashSale = false;

        // 1. Dynamic Pricing
        var dynamicPricings = promotions.Where(p => p.PromotionTypeId == PromotionTypes.Ids.DynamicPricing).ToList();
        foreach (var dp in dynamicPricings)
        {
            var discount = ApplyDynamicPricing(dp, currentPrice, startDate);
            if (discount != null)
            {
                currentPrice -= discount.DiscountAmount;
                appliedDiscounts.Add(discount);
            }
        }

        // 2. Early Bird
        var earlyBirds = promotions.Where(p => p.PromotionTypeId == PromotionTypes.Ids.EarlyBird).ToList();
        foreach (var eb in earlyBirds)
        {
            var daysAhead = (startDate - DateTime.UtcNow).TotalDays;
            if (eb.MinDaysAhead.HasValue && daysAhead >= eb.MinDaysAhead.Value)
            {
                var discount = CalculateDiscount(eb, currentPrice);
                if (discount != null)
                {
                    currentPrice -= discount.DiscountAmount;
                    appliedDiscounts.Add(discount);
                }
            }
        }

        // 2b. Last Minute
        var lastMinutes = promotions.Where(p => p.PromotionTypeId == PromotionTypes.Ids.LastMinute).ToList();
        foreach (var lm in lastMinutes)
        {
            var hoursBeforeStart = (startDate - DateTime.UtcNow).TotalHours;
            if (lm.MaxHoursBeforeStart.HasValue && hoursBeforeStart <= lm.MaxHoursBeforeStart.Value && hoursBeforeStart > 0)
            {
                var discount = CalculateDiscount(lm, currentPrice);
                if (discount != null)
                {
                    currentPrice -= discount.DiscountAmount;
                    appliedDiscounts.Add(discount);
                }
            }
        }

        // 3. Flash Sale (Flash Sale + Kupon birlikte uygulanmaz)
        var flashSales = promotions.Where(p => p.PromotionTypeId == PromotionTypes.Ids.FlashSale).ToList();
        foreach (var fs in flashSales)
        {
            if (fs.FlashSaleStock == null || fs.FlashSaleSoldCount < fs.FlashSaleStock)
            {
                var discount = CalculateDiscount(fs, currentPrice);
                if (discount != null)
                {
                    currentPrice -= discount.DiscountAmount;
                    appliedDiscounts.Add(discount);
                    hasFlashSale = true;
                }
            }
        }

        // 4. Group Discount
        var groupDiscounts = promotions.Where(p => p.PromotionTypeId == PromotionTypes.Ids.GroupDiscount).ToList();
        foreach (var gd in groupDiscounts)
        {
            var minSize = gd.MinGroupSize ?? 2;
            var maxSize = gd.MaxGroupSize ?? int.MaxValue;
            if (numberOfPeople >= minSize && numberOfPeople <= maxSize)
            {
                var discount = CalculateDiscount(gd, currentPrice);
                if (discount != null)
                {
                    currentPrice -= discount.DiscountAmount;
                    appliedDiscounts.Add(discount);
                }
            }
        }

        // 5. Coupon (Flash Sale varsa kupon uygulanmaz)
        if (!hasFlashSale && !string.IsNullOrEmpty(couponCode))
        {
            var coupon = await _promotionService.GetByCodeAsync(tour.CompanyId, couponCode);
            if (coupon != null && IsApplicableToTour(coupon, tourId))
            {
                var couponValid = await ValidateCouponInternal(coupon, currentPrice, visitorId);
                if (couponValid.valid)
                {
                    var discount = CalculateDiscount(coupon, currentPrice);
                    if (discount != null)
                    {
                        currentPrice -= discount.DiscountAmount;
                        appliedDiscounts.Add(discount);
                    }
                }
            }
        }

        // Max indirim kontrolu (%50)
        var totalDiscount = subTotal - currentPrice;
        var maxDiscount = subTotal * MaxTotalDiscountPercent;
        if (totalDiscount > maxDiscount)
        {
            totalDiscount = maxDiscount;
            currentPrice = subTotal - totalDiscount;
        }

        if (currentPrice < 0) currentPrice = 0;

        var depositPercentage = tour.Company.DepositPercentage;
        var depositAmount = currentPrice * depositPercentage / 100;

        return new PriceCalculationResult
        {
            BasePrice = basePrice,
            NumberOfPeople = numberOfPeople,
            SubTotal = subTotal,
            TotalDiscount = totalDiscount,
            FinalPrice = currentPrice,
            DepositAmount = depositAmount,
            DepositPercentage = depositPercentage,
            AppliedDiscounts = appliedDiscounts
        };
    }

    public async Task<(bool valid, decimal discountAmount, string? errorMessage)> ValidateCouponAsync(
        string code, int tourId, int numberOfPeople, decimal totalPrice, int? visitorId = null)
    {
        var tour = await _tourService.GetByIdWithCompanyAsync(tourId);
        if (tour == null)
            return (false, 0, "Tur bulunamadi");

        var coupon = await _promotionService.GetByCodeAsync(tour.CompanyId, code);
        if (coupon == null)
            return (false, 0, "Gecersiz kupon kodu");

        if (!IsApplicableToTour(coupon, tourId))
            return (false, 0, "Bu kupon bu tur icin gecerli degil");

        var validation = await ValidateCouponInternal(coupon, totalPrice, visitorId);
        if (!validation.valid)
            return (false, 0, validation.errorMessage);

        var discount = CalculateDiscount(coupon, totalPrice);
        if (discount == null)
            return (false, 0, "Indirim hesaplanamadi");

        return (true, discount.DiscountAmount, null);
    }

    public async Task<object> GetTourPromotionBadgesAsync(int tourId, DateTime? tourStartDate = null)
    {
        var tour = await _tourService.GetByIdWithCompanyAsync(tourId);
        if (tour == null)
            return new List<object>();

        var promotions = await _promotionService.GetActiveByCompanyId(tour.CompanyId).ToListAsync();
        promotions = promotions.Where(p => IsApplicableToTour(p, tourId)).ToList();

        var badges = new List<object>();
        var startDate = tourStartDate ?? DateTime.UtcNow.AddDays(7);

        foreach (var p in promotions)
        {
            var badge = GetBadgeForPromotion(p, startDate);
            if (badge != null)
                badges.Add(badge);
        }

        return badges;
    }

    public async Task<object> GetActiveFlashSalesAsync()
    {
        var flashSales = await _promotionService.GetActiveFlashSales()
            .OrderBy(p => p.EndDate).ToListAsync();

        return flashSales.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.DiscountValue,
            discountType = DiscountTypes.GetById(p.DiscountTypeId)?.SystemName,
            p.EndDate,
            p.FlashSaleStock,
            p.FlashSaleSoldCount,
            remaining = p.FlashSaleStock.HasValue ? p.FlashSaleStock.Value - p.FlashSaleSoldCount : (int?)null,
            companyName = p.Company?.Name,
            p.ApplicableTourIds
        });
    }

    public async Task<object> GetLastMinuteDealsAsync()
    {
        var deals = await _promotionService.GetActiveLastMinuteDeals()
            .OrderBy(p => p.EndDate).ToListAsync();

        return deals.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.DiscountValue,
            discountType = DiscountTypes.GetById(p.DiscountTypeId)?.SystemName,
            p.EndDate,
            p.MaxHoursBeforeStart,
            companyName = p.Company?.Name,
            p.ApplicableTourIds
        });
    }

    // Private helpers

    private static bool IsApplicableToTour(Promotion promotion, int tourId)
    {
        if (string.IsNullOrEmpty(promotion.ApplicableTourIds))
            return true; // null = tum turlar

        try
        {
            var tourIds = JsonSerializer.Deserialize<List<int>>(promotion.ApplicableTourIds);
            return tourIds != null && tourIds.Contains(tourId);
        }
        catch
        {
            return true;
        }
    }

    private async Task<(bool valid, string? errorMessage)> ValidateCouponInternal(
        Promotion coupon, decimal totalPrice, int? visitorId)
    {
        // Kullanim limiti kontrolu
        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            return (false, "Bu kuponun kullanim limiti dolmus");

        // Kullanici basi limit kontrolu
        if (visitorId.HasValue && coupon.UsageLimitPerUser.HasValue)
        {
            var userUsageCount = await _promotionService.GetUsageCountByVisitorAsync(coupon.Id, visitorId.Value);
            if (userUsageCount >= coupon.UsageLimitPerUser.Value)
                return (false, "Bu kuponu daha fazla kullanamazsiniz");
        }

        // Minimum siparis tutari kontrolu
        if (coupon.MinOrderAmount.HasValue && totalPrice < coupon.MinOrderAmount.Value)
            return (false, $"Minimum siparis tutari {coupon.MinOrderAmount.Value:N2} TL");

        return (true, null);
    }

    private static AppliedDiscount? CalculateDiscount(Promotion promotion, decimal currentPrice)
    {
        decimal discountAmount = 0;

        switch (promotion.DiscountTypeId)
        {
            case 0: // Percentage
                discountAmount = currentPrice * promotion.DiscountValue / 100;
                break;
            case 1: // FixedAmount
                discountAmount = promotion.DiscountValue;
                break;
            case 2: // Multiplier
                discountAmount = currentPrice - (currentPrice * promotion.DiscountValue);
                break;
        }

        // Max indirim tutari kontrolu
        if (promotion.MaxDiscountAmount.HasValue && discountAmount > promotion.MaxDiscountAmount.Value)
            discountAmount = promotion.MaxDiscountAmount.Value;

        if (discountAmount <= 0) return null;
        if (discountAmount > currentPrice) discountAmount = currentPrice;

        return new AppliedDiscount
        {
            PromotionId = promotion.Id,
            PromotionName = promotion.Name,
            PromotionType = PromotionTypes.GetById(promotion.PromotionTypeId)?.SystemName ?? "Unknown",
            DiscountType = DiscountTypes.GetById(promotion.DiscountTypeId)?.SystemName ?? "Unknown",
            DiscountValue = promotion.DiscountValue,
            DiscountAmount = Math.Round(discountAmount, 2),
            Rule = $"{PromotionTypes.GetById(promotion.PromotionTypeId)?.SystemName}: {promotion.Name}"
        };
    }

    private static AppliedDiscount? ApplyDynamicPricing(Promotion dp, decimal currentPrice, DateTime startDate)
    {
        decimal multiplier = 1;
        string rule = "";

        // Hafta sonu carpani
        if (dp.WeekendMultiplier.HasValue && (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday))
        {
            multiplier = dp.WeekendMultiplier.Value;
            rule = "Weekend";
        }

        // Sezon kurallari (JSON)
        if (!string.IsNullOrEmpty(dp.SeasonRules))
        {
            try
            {
                var seasons = JsonSerializer.Deserialize<List<SeasonRule>>(dp.SeasonRules);
                if (seasons != null)
                {
                    foreach (var season in seasons)
                    {
                        if (startDate.Month >= season.StartMonth && startDate.Month <= season.EndMonth)
                        {
                            multiplier = season.Multiplier;
                            rule = season.Name ?? "Season";
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        if (multiplier == 1) return null;

        // Multiplier < 1 ise indirim, > 1 ise artis (artis icin negatif discount)
        var discountAmount = currentPrice - (currentPrice * multiplier);
        if (discountAmount <= 0) return null; // Sadece indirimler

        return new AppliedDiscount
        {
            PromotionId = dp.Id,
            PromotionName = dp.Name,
            PromotionType = "DynamicPricing",
            DiscountType = "Multiplier",
            DiscountValue = multiplier,
            DiscountAmount = Math.Round(discountAmount, 2),
            Rule = $"DynamicPricing: {rule}"
        };
    }

    private static object? GetBadgeForPromotion(Promotion p, DateTime startDate)
    {
        var type = PromotionTypes.GetById(p.PromotionTypeId);
        if (type == null) return null;

        switch (p.PromotionTypeId)
        {
            case 1: // EarlyBird
                var daysAhead = (startDate - DateTime.UtcNow).TotalDays;
                if (p.MinDaysAhead.HasValue && daysAhead >= p.MinDaysAhead.Value)
                    return new { type = type.SystemName, icon = type.Icon, cssClass = type.CssClass, label = $"%{p.DiscountValue} Erken Rez.", nameKey = "Badge.EarlyBird" };
                break;
            case 2: // LastMinute
                return new { type = type.SystemName, icon = type.Icon, cssClass = type.CssClass, label = $"%{p.DiscountValue} Son Dakika", nameKey = "Badge.LastMinute" };
            case 3: // GroupDiscount
                return new { type = type.SystemName, icon = type.Icon, cssClass = type.CssClass, label = $"%{p.DiscountValue} Grup Ind.", nameKey = "Badge.GroupDiscount" };
            case 4: // FlashSale
                if (p.FlashSaleStock == null || p.FlashSaleSoldCount < p.FlashSaleStock)
                    return new { type = type.SystemName, icon = type.Icon, cssClass = type.CssClass, label = $"%{p.DiscountValue} Flash Sale", nameKey = "Badge.FlashSale", endDate = p.EndDate };
                break;
        }
        return null;
    }
}

internal class SeasonRule
{
    public int StartMonth { get; set; }
    public int EndMonth { get; set; }
    public decimal Multiplier { get; set; }
    public string? Name { get; set; }
}
