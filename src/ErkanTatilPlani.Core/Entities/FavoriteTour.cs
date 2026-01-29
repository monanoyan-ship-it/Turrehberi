namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Kullanicinin favori turlari
/// </summary>
public class FavoriteTour : BaseEntity
{
    public int VisitorId { get; set; }
    public virtual Visitor Visitor { get; set; } = null!;

    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;
}
