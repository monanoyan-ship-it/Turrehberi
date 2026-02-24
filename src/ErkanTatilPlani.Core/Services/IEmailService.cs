namespace ErkanTatilPlani.Core.Services;

public interface IEmailService
{
    // Mevcut metodlar (geriye uyumlu - default hesap kullanır)
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);

    // Yeni overload - hesap ismi ile
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string accountName);

    // Rezervasyon emailleri
    Task SendReservationConfirmedEmailAsync(ReservationEmailModel model);
    Task SendReservationCancelledEmailAsync(ReservationEmailModel model);
    Task SendReservationRejectedEmailAsync(ReservationEmailModel model);

    // Şifre sıfırlama ve email doğrulama
    Task SendPasswordResetEmailAsync(string toEmail, string resetUrl, string customerName, string language);
    Task SendEmailVerificationAsync(string toEmail, string verifyUrl, string customerName, string language);

    // Template tabanlı email gonderimi (DB'den sablon okur)
    Task SendTemplateEmailAsync(string toEmail, string templateKey, string language, Dictionary<string, string> placeholders);

    // Cache invalidation (admin guncelleme sonrasi cagirilir)
    void InvalidateCache();
}

public class ReservationEmailModel
{
    public string ToEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TourName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationValue { get; set; }
    public int DurationUnitId { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public string PreferredLanguage { get; set; } = "tr";
}

/// <summary>
/// Tek bir email hesabının ayarları
/// </summary>
public class EmailAccountSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

/// <summary>
/// Çoklu email hesabı desteği için ayarlar
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// İsimli email hesapları (örn: "default", "reservations", "support")
    /// </summary>
    public Dictionary<string, EmailAccountSettings> Accounts { get; set; } = new();

    /// <summary>
    /// Hesap belirtilmediğinde kullanılacak varsayılan hesap ismi
    /// </summary>
    public string DefaultAccount { get; set; } = "default";
}
