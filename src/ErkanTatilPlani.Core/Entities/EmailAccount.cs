namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Email hesap ayarlarini saklayan entity.
/// SMTP sunucu bilgileri ve gonderen ayarlari burada tutulur.
/// </summary>
public class EmailAccount : BaseEntity
{
    /// <summary>
    /// Hesap ismi (unique, ornekler: "default", "reservations", "support")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hesap aciklamasi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// SMTP sunucu adresi
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP port numarasi
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP kullanici adi
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP sifre (sifreli saklanmali)
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gonderen email adresi
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gonderen ismi
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// SSL kullanilsin mi
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Varsayilan hesap mi (sadece bir hesap varsayilan olabilir)
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gosterim sirasi
    /// </summary>
    public int DisplayOrder { get; set; }

    // Navigation properties
    public virtual ICollection<EmailTemplate> EmailTemplates { get; set; } = new List<EmailTemplate>();
}
