namespace ErkanTatilPlani.Core.Entities;

public class TourWatch : BaseEntity
{
    public int VisitorId { get; set; }
    public int TourId { get; set; }
    public int WatchDays { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool NotifyScarcity { get; set; } = true;
    public bool NotifyPriceChange { get; set; } = true;
    public bool NotifyNewDate { get; set; } = true;

    public virtual Visitor Visitor { get; set; } = null!;
    public virtual Tour Tour { get; set; } = null!;
}
