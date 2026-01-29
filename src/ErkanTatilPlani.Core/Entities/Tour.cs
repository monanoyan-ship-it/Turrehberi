namespace ErkanTatilPlani.Core.Entities;

public class Tour : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int MaxCapacity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; } = false;

    // ===============================================
    // KONUM BILGISI (Harita icin)
    // ===============================================

    /// <summary>
    /// Tur lokasyonunun enlemi
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Tur lokasyonunun boylamı
    /// </summary>
    public double? Longitude { get; set; }

    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

    // ===============================================
    // YORUM ISTATISTIKLERI (Cache - performans icin)
    // ===============================================

    /// <summary>
    /// Toplam onaylanmis yorum sayisi
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Ortalama puan (1.0 - 5.0)
    /// </summary>
    public decimal AverageRating { get; set; }

    // ===============================================
    // ILISKILER
    // ===============================================

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<TourReview> Reviews { get; set; } = new List<TourReview>();
    public virtual ICollection<FavoriteTour> FavoritedBy { get; set; } = new List<FavoriteTour>();
}
