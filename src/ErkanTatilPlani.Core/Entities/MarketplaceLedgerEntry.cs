namespace ErkanTatilPlani.Core.Entities;

public class MarketplaceLedgerEntry : BaseEntity
{
    public int? PaymentTransactionId { get; set; }
    public int? ReservationId { get; set; }
    public int? CompanyId { get; set; }
    public int? PayoutBatchId { get; set; }
    public int EntryTypeId { get; set; }
    public int StatusId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? AvailableAt { get; set; }
    public DateTime? SettledAt { get; set; }

    public virtual PaymentTransaction? PaymentTransaction { get; set; }
    public virtual Reservation? Reservation { get; set; }
    public virtual Company? Company { get; set; }
    public virtual PayoutBatch? PayoutBatch { get; set; }
}
