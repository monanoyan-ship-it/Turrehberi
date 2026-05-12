namespace ErkanTatilPlani.Core.Entities;

public class PaymentLineItem : BaseEntity
{
    public int PaymentTransactionId { get; set; }
    public int ReservationId { get; set; }
    public int CompanyId { get; set; }

    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? ProviderPaymentTransactionId { get; set; }
    public int ProviderTransactionStatus { get; set; }
    public string? SubMerchantKey { get; set; }
    public string? ExternalSubMerchantId { get; set; }

    public decimal Price { get; set; }
    public decimal PaidPrice { get; set; }
    public decimal SubMerchantPrice { get; set; }
    public decimal SubMerchantPayoutRate { get; set; }
    public decimal SubMerchantPayoutAmount { get; set; }
    public decimal MerchantPayoutAmount { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal IyziCommissionRateAmount { get; set; }
    public decimal IyziCommissionFee { get; set; }
    public decimal BlockageRate { get; set; }
    public decimal BlockageRateAmountMerchant { get; set; }
    public decimal BlockageRateAmountSubMerchant { get; set; }
    public decimal WithholdingTax { get; set; }
    public DateTime? BlockageResolvedDate { get; set; }

    public virtual PaymentTransaction PaymentTransaction { get; set; } = null!;
    public virtual Reservation Reservation { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
}
