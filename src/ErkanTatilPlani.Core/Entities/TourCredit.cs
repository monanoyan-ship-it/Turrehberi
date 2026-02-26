using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class TourCredit : BaseEntity
{
    public int VisitorId { get; set; }
    public int TransactionTypeId { get; set; } = CreditTransactionTypes.Ids.Earn;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public int? ReservationId { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public virtual Visitor Visitor { get; set; } = null!;
    public virtual Reservation? Reservation { get; set; }
}
