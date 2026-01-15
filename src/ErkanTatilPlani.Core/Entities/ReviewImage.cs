namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Yoruma eklenen fotograflar
/// </summary>
public class ReviewImage : BaseEntity
{
    /// <summary>
    /// Bagli oldugu yorum
    /// </summary>
    public int ReviewId { get; set; }
    public virtual TourReview Review { get; set; } = null!;

    /// <summary>
    /// Resim URL (yuklendiginde)
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Kucuk resim URL (thumbnail)
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// Resim aciklamasi/caption
    /// </summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// Siralama (gosterim sirasi)
    /// </summary>
    public int DisplayOrder { get; set; }

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

    /// <summary>
    /// Alt metin (erisilebilirlik)
    /// </summary>
    public string AltText { get; set; } = string.Empty;
}
