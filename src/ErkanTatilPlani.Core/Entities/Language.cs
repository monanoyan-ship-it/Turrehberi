namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Sistem dilleri
/// </summary>
public class Language : BaseEntity
{
    /// <summary>
    /// Dil adi (Turkce, English, Deutsch, etc.)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dil kodu (tr-TR, en-US, de-DE, ru-RU, es-ES)
    /// </summary>
    public string LanguageCulture { get; set; } = string.Empty;

    /// <summary>
    /// Kisa kod - SEO icin (tr, en, de, ru, es)
    /// </summary>
    public string UniqueSeoCode { get; set; } = string.Empty;

    /// <summary>
    /// Bayrak ikonu (bi-flag icin veya flag-icon-tr gibi)
    /// </summary>
    public string? FlagIcon { get; set; }

    /// <summary>
    /// Sagdan sola yazim (Arapca vb. icin)
    /// </summary>
    public bool Rtl { get; set; } = false;

    /// <summary>
    /// Varsayilan dil mi?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Siralama
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

}
