namespace ErkanTatilPlani.Core.Entities;

public class AbandonedCart : BaseEntity
{
    public int? VisitorId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int TourId { get; set; }
    public int? ScheduleId { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal Price { get; set; }
    public string? DateToken { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public bool Recovered { get; set; }
    public int? ReservationId { get; set; }

    public virtual Visitor? Visitor { get; set; }
    public virtual Tour Tour { get; set; } = null!;
    public virtual TourSchedule? Schedule { get; set; }
    public virtual Reservation? Reservation { get; set; }
}
