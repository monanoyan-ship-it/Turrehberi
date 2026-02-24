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

    // ===============================================
    // GELISMIS OZELLIKLER (Faz 8)
    // ===============================================

    /// <summary>
    /// Zorluk seviyesi (TourDifficulties TypeDefinition)
    /// </summary>
    public int DifficultyId { get; set; } = 1; // Moderate (default)

    /// <summary>
    /// Kategori (TourCategories TypeDefinition)
    /// </summary>
    public int CategoryId { get; set; } = 0; // Adventure (default)

    /// <summary>
    /// Rehber dilleri (virgullu ayrilmis: "tr,en,de")
    /// </summary>
    public string? GuideLanguages { get; set; }

    /// <summary>
    /// Dahil olan hizmetler (her satir bir madde, newline ile ayrilmis)
    /// </summary>
    public string? Inclusions { get; set; }

    /// <summary>
    /// Dahil olmayan hizmetler (her satir bir madde, newline ile ayrilmis)
    /// </summary>
    public string? Exclusions { get; set; }

    // ===============================================
    // BULUSMA NOKTASI
    // ===============================================

    public double? MeetingPointLat { get; set; }
    public double? MeetingPointLng { get; set; }
    public string? MeetingPointAddress { get; set; }

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
    public virtual ICollection<TourDate> TourDates { get; set; } = new List<TourDate>();
    public virtual ICollection<TourSchedule> Schedules { get; set; } = new List<TourSchedule>();
    public virtual ICollection<TourWatch> TourWatches { get; set; } = new List<TourWatch>();
    public virtual ICollection<TourPhoto> Photos { get; set; } = new List<TourPhoto>();
}
