namespace ErkanTatilPlani.Core.Entities;

public class PaymentTransaction : BaseEntity
{
    public int ReservationId { get; set; }
    public int CompanyId { get; set; }
    public int VisitorId { get; set; }

    public int TypeId { get; set; }
    public int StatusId { get; set; }

    public string Provider { get; set; } = "Iyzico";
    public string PaymentMethodSystemName { get; set; } = "iyzico-card";
    public string Currency { get; set; } = "TRY";
    public string ConversationId { get; set; } = string.Empty;
    public string? PaymentId { get; set; }
    public string? PaymentToken { get; set; }
    public string? BuyerIp { get; set; }

    public decimal GrossAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal SellerReceivableAmount { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal PlatformCommissionRate { get; set; }
    public decimal IyziCommissionRateAmount { get; set; }
    public decimal IyziCommissionFee { get; set; }
    public decimal WithholdingTax { get; set; }
    public decimal RefundedAmount { get; set; }

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? CallbackReceivedAt { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
    public virtual Visitor Visitor { get; set; } = null!;
    public virtual ICollection<PaymentLineItem> LineItems { get; set; } = new List<PaymentLineItem>();
    public virtual ICollection<MarketplaceLedgerEntry> LedgerEntries { get; set; } = new List<MarketplaceLedgerEntry>();
    public virtual ICollection<MarketplaceRefund> Refunds { get; set; } = new List<MarketplaceRefund>();
}
