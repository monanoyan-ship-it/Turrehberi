namespace ErkanTatilPlani.Core.Entities;

public class Reservation : BaseEntity
{
    public int TourId { get; set; }
    public int VisitorId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public string Notes { get; set; } = string.Empty;

    // Odeme Bilgileri
    public string? PaymentId { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;
    public DateTime? PaidAt { get; set; }
    public string? PaymentToken { get; set; }

    public virtual Tour Tour { get; set; } = null!;
    public virtual Visitor Visitor { get; set; } = null!;
}

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public enum PaymentStatusEnum
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}
