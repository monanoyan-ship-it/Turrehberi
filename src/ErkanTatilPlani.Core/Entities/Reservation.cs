using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class Reservation : BaseEntity
{
    public int TourId { get; set; }
    public int VisitorId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationValue { get; set; }
    public int DurationUnitId { get; set; }
    public decimal UnitPrice { get; set; }
    public int? ScheduleId { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; } = ReservationStatuses.Ids.Pending;
    public string Notes { get; set; } = string.Empty;

    // Odeme Bilgileri
    public decimal DepositAmount { get; set; }  // Gereken on odeme tutari
    public decimal PaidAmount { get; set; }     // Odenen toplam tutar
    public string? PaymentId { get; set; }
    public int PaymentStatus { get; set; } = PaymentStatuses.Ids.Pending;
    public DateTime? PaidAt { get; set; }
    public string? PaymentToken { get; set; }

    // Promosyon Bilgileri
    public decimal DiscountAmount { get; set; }
    public int? PromotionId { get; set; }
    public string? AppliedPromotions { get; set; }
    public string? CouponCode { get; set; }

    // Sadakat Sistemi
    public decimal CreditUsed { get; set; }

    // Katilimci ve Bilet Bilgileri
    public string? ParticipantInfo { get; set; }
    public string? QrCode { get; set; }
    public string? QrToken { get; set; }
    public string? PhotoLink { get; set; }

    public virtual Tour Tour { get; set; } = null!;
    public virtual Visitor Visitor { get; set; } = null!;
    public virtual TourSchedule? Schedule { get; set; }
    public virtual Promotion? Promotion { get; set; }
}
