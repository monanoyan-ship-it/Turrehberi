namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Firma galeri fotograflari
/// </summary>
public class CompanyGalleryImage : BaseEntity
{
    /// <summary>
    /// Bagli oldugu firma
    /// </summary>
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

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
    /// Resim aciklamasi
    /// </summary>
    public string Description { get; set; } = string.Empty;

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

    /// <summary>
    /// Vitrin resmi mi? (Ana profil sayfasinda gosterilir)
    /// </summary>
    public bool IsFeatured { get; set; }
}
