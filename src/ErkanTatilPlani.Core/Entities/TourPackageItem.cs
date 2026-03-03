namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Tur paketi icindeki tur (Faz 15.6)
/// </summary>
public class TourPackageItem : BaseEntity
{
    public int TourPackageId { get; set; }
    public virtual TourPackage TourPackage { get; set; } = null!;

    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;

    /// <summary>
    /// Sira numarasi
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Bu tur icin kisi sayisi
    /// </summary>
    public int NumberOfPeople { get; set; } = 1;
}
