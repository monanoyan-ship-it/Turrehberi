namespace ErkanTatilPlani.Core.Entities;

public class MarketplaceRefund : BaseEntity
{
    public int PaymentTransactionId { get; set; }
    public int ReservationId { get; set; }
    public int CompanyId { get; set; }
    public int StatusId { get; set; }
    public int? RequestedById { get; set; }
    public int? ProcessedById { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Reason { get; set; } = string.Empty;
    public string? ProviderRefundId { get; set; }
    public string? ProviderPaymentTransactionId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public virtual PaymentTransaction PaymentTransaction { get; set; } = null!;
    public virtual Reservation Reservation { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
    public virtual Visitor? RequestedBy { get; set; }
    public virtual Visitor? ProcessedBy { get; set; }
}
