namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Tur sonrasi dijital icerik - firma tarafindan saglanan (Faz 15.7)
/// Sesli rehber, rota bilgisi, PDF rehber vb.
/// </summary>
public class TourDigitalContent : BaseEntity
{
    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Icerik tipi: AudioGuide, RouteMap, PDF, Video, Gallery
    /// </summary>
    public int ContentTypeId { get; set; }

    /// <summary>
    /// Dosya URL'si veya icerik metni
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// Metin icerigi (HTML)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Sira numarasi
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Sadece rezervasyon yapanlara acik mi?
    /// </summary>
    public bool RequiresReservation { get; set; } = true;
}
