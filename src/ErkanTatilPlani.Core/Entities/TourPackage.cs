namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Kullanici tarafindan olusturulan tur paketi (Faz 15.6)
/// Birden fazla turu birlikte paketleme
/// </summary>
public class TourPackage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Paketi olusturan ziyaretci
    /// </summary>
    public int VisitorId { get; set; }
    public virtual Visitor Visitor { get; set; } = null!;

    /// <summary>
    /// Toplam fiyat (indirimsiz)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Paket indirimi (yuzde)
    /// </summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>
    /// Indirimli fiyat
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Paket herkese acik mi?
    /// </summary>
    public bool IsPublic { get; set; }

    public virtual ICollection<TourPackageItem> Items { get; set; } = new List<TourPackageItem>();
}
