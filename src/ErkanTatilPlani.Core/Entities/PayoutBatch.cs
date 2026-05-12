namespace ErkanTatilPlani.Core.Entities;

public class PayoutBatch : BaseEntity
{
    public int CompanyId { get; set; }
    public int StatusId { get; set; }
    public int? ApprovedById { get; set; }

    public string BatchNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "TRY";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public decimal GrossAmount { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? BankReference { get; set; }
    public string? Notes { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public virtual Company Company { get; set; } = null!;
    public virtual Visitor? ApprovedBy { get; set; }
    public virtual ICollection<MarketplaceLedgerEntry> LedgerEntries { get; set; } = new List<MarketplaceLedgerEntry>();
}
