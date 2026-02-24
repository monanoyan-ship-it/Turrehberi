namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Takvim sablonu - firma bir schedule tanimlar, sistem tarihleri dinamik uretir.
/// Rezervasyon yapildiginda dogrudan Reservation tablosuna yazilir.
/// </summary>
public class TourSchedule : BaseEntity
{
    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;

    /// <summary>
    /// Haftanin gunleri - JSON int array: [1,3,5] = Pzt/Car/Cum
    /// DayOfWeek: 0=Pazar, 1=Pazartesi, ..., 6=Cumartesi
    /// </summary>
    public string DaysOfWeekJson { get; set; } = "[]";

    /// <summary>
    /// Seans baslangic saati
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Sure miktari
    /// </summary>
    public int DurationValue { get; set; } = 1;

    /// <summary>
    /// Sure birimi (DurationUnits TypeDef: Hour=1, Day=2, Week=3, Month=4)
    /// </summary>
    public int DurationUnitId { get; set; } = 2;

    /// <summary>
    /// Bu schedule icin ozel fiyat (null ise turun varsayilan fiyati kullanilir)
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Bu schedule icin ozel kapasite (null ise turun varsayilan kapasitesi kullanilir)
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Gecerlilik baslangic tarihi
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Gecerlilik bitis tarihi
    /// </summary>
    public DateTime ValidTo { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<TourGuideAssignment> GuideAssignments { get; set; } = new List<TourGuideAssignment>();
}
