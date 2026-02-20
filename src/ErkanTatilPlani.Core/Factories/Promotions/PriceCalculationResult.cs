namespace ErkanTatilPlani.Core.Factories.Promotions;

public class PriceCalculationResult
{
    public decimal BasePrice { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal DepositAmount { get; set; }
    public int DepositPercentage { get; set; }
    public List<AppliedDiscount> AppliedDiscounts { get; set; } = new();
}

public class AppliedDiscount
{
    public int PromotionId { get; set; }
    public string PromotionName { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Rule { get; set; } = string.Empty;
}
