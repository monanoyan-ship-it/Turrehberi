namespace ErkanTatilPlani.Core.Entities;

public class PromotionUsage : BaseEntity
{
    public int PromotionId { get; set; }
    public int ReservationId { get; set; }
    public int? VisitorId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string AppliedRule { get; set; } = string.Empty;

    // Navigation
    public virtual Promotion Promotion { get; set; } = null!;
    public virtual Reservation Reservation { get; set; } = null!;
    public virtual Visitor? Visitor { get; set; }
}
