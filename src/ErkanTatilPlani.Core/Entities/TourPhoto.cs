namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Tur fotograflari
/// </summary>
public class TourPhoto : BaseEntity
{
    /// <summary>
    /// Bagli oldugu tur
    /// </summary>
    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;

    /// <summary>
    /// Resim URL (yuklendiginde)
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Kucuk resim URL (thumbnail)
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// Resim basligi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Siralama (gosterim sirasi)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Kapak fotografı mi?
    /// </summary>
    public bool IsCover { get; set; }

    /// <summary>
    /// Dosya boyutu (byte)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Resim genisligi (pixel)
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Resim yuksekligi (pixel)
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// MIME tipi (image/jpeg, image/png, vb.)
    /// </summary>
    public string MimeType { get; set; } = string.Empty;
}
