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
    public decimal DepositAmount { get; set; }  // Gereken on odeme tutari
    public decimal PaidAmount { get; set; }     // Odenen toplam tutar
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
    DepositPaid = 1,   // On odeme yapildi
    FullyPaid = 2,     // Tam odeme yapildi
    Failed = 3,
    Refunded = 4
}
