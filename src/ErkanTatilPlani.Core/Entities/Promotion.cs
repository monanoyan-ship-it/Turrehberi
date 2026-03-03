using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class Promotion : BaseEntity
{
    // Temel alanlar
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PromotionTypeId { get; set; } = PromotionTypes.Ids.Coupon;
    public int DiscountTypeId { get; set; } = DiscountTypes.Ids.Percentage;
    public decimal DiscountValue { get; set; }
    public int StatusId { get; set; } = PromotionStatuses.Ids.Active;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CreatedByVisitorId { get; set; }

    // Kupon (10.1)
    public string? Code { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    // Erken Rezervasyon (10.2)
    public int? MinDaysAhead { get; set; }

    // Son Dakika (10.3)
    public int? MaxHoursBeforeStart { get; set; }
    public int? MinAvailableCapacityPercent { get; set; }

    // Grup Indirimi (10.4)
    public int? MinGroupSize { get; set; }
    public int? MaxGroupSize { get; set; }

    // Flash Sale (10.5)
    public int? FlashSaleStock { get; set; }
    public int FlashSaleSoldCount { get; set; }

    // Paket (10.6)
    public string? BundleTourIds { get; set; }
    public int? MinBundleTourCount { get; set; }

    // Dinamik Fiyatlandirma (10.7)
    public string? SeasonRules { get; set; }
    public decimal? WeekendMultiplier { get; set; }
    public decimal? HighDemandMultiplier { get; set; }
    public int? HighDemandThresholdPercent { get; set; }

    // Toplu Alim Indirimi (10.8)
    // VolumeRules JSON: [{"minSold":10,"discountPercent":5},{"minSold":20,"discountPercent":10},{"minSold":50,"discountPercent":15}]
    public string? VolumeRules { get; set; }

    // Ortak
    public string? ApplicableTourIds { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual Visitor CreatedByVisitor { get; set; } = null!;
    public virtual ICollection<PromotionUsage> Usages { get; set; } = new List<PromotionUsage>();
}
