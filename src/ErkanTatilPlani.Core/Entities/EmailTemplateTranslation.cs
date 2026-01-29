namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Email sablon cevirisi entity.
/// Her dil icin ayri konu ve icerik.
/// </summary>
public class EmailTemplateTranslation : BaseEntity
{
    /// <summary>
    /// Bagli oldugu sablon
    /// </summary>
    public int EmailTemplateId { get; set; }

    /// <summary>
    /// Dil kodu (ornekler: "tr", "en", "ru", "de", "es", "fr", "ar", "fa", "pt")
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Email konusu (placeholder destekli)
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email icerigi - HTML (placeholder destekli)
    /// </summary>
    public string Body { get; set; } = string.Empty;

    // Navigation property
    public virtual EmailTemplate EmailTemplate { get; set; } = null!;
}
