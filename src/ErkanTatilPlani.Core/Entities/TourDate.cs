namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Tur musaitlik takvimi - tarih bazli fiyat ve kontenjan yonetimi
/// </summary>
public class TourDate : BaseEntity
{
    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;

    /// <summary>
    /// Tur baslangic tarihi
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Tur bitis tarihi
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Bu tarih icin ozel fiyat (null ise turun varsayilan fiyati kullanilir)
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Bu tarih icin ozel kapasite (null ise turun varsayilan kapasitesi kullanilir)
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Rezerve edilen kisi sayisi
    /// </summary>
    public int BookedCount { get; set; }

    /// <summary>
    /// Musait mi?
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
