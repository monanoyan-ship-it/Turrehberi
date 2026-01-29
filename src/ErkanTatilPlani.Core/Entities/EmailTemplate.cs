namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Email sablonu entity.
/// Sistem sablonlari (password_reset, email_verification, vs.) ve kullanici tanimli sablonlar.
/// </summary>
public class EmailTemplate : BaseEntity
{
    /// <summary>
    /// Sablon anahtari (unique, ornekler: "password_reset", "email_verification", "reservation_confirmed")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Sablon adi (UI gosterimi icin)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sablon aciklamasi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bu sablon icin kullanilacak email hesabi (null ise varsayilan hesap kullanilir)
    /// </summary>
    public int? EmailAccountId { get; set; }

    /// <summary>
    /// Sablonda kullanilabilecek placeholder'lar (JSON array: ["{customerName}", "{resetUrl}"])
    /// </summary>
    public string? Placeholders { get; set; }

    /// <summary>
    /// Sistem sablonu mu (silinemez)
    /// </summary>
    public bool IsSystemTemplate { get; set; }

    // Navigation properties
    public virtual EmailAccount? EmailAccount { get; set; }
    public virtual ICollection<EmailTemplateTranslation> Translations { get; set; } = new List<EmailTemplateTranslation>();
}
