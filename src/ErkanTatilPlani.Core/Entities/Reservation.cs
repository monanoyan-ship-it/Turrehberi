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
