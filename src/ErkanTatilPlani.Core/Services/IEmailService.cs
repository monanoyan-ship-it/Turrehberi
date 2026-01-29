namespace ErkanTatilPlani.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    Task SendReservationConfirmedEmailAsync(ReservationEmailModel model);
    Task SendReservationCancelledEmailAsync(ReservationEmailModel model);
    Task SendReservationRejectedEmailAsync(ReservationEmailModel model);
}

public class ReservationEmailModel
{
    public string ToEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string TourName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public string PreferredLanguage { get; set; } = "tr";
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
