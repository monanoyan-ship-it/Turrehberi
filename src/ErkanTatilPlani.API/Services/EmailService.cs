using System.Net;
using System.Net.Mail;
using ErkanTatilPlani.Core.Localization;
using ErkanTatilPlani.Core.Services;
using Microsoft.Extensions.Options;

namespace ErkanTatilPlani.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILocalizationService _localizer;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILocalizationService localizer,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw - email failure shouldn't break the main flow
        }
    }

    public async Task SendReservationConfirmedEmailAsync(ReservationEmailModel model)
    {
        _localizer.SetCulture(model.PreferredLanguage);

        var subject = _localizer.T("Email.ReservationConfirmed.Subject");
        var htmlBody = BuildReservationConfirmedEmail(model);

        await SendEmailAsync(model.ToEmail, subject, htmlBody);
    }

    public async Task SendReservationCancelledEmailAsync(ReservationEmailModel model)
    {
        _localizer.SetCulture(model.PreferredLanguage);

        var subject = _localizer.T("Email.ReservationCancelled.Subject");
        var htmlBody = BuildReservationCancelledEmail(model);

        await SendEmailAsync(model.ToEmail, subject, htmlBody);
    }

    public async Task SendReservationRejectedEmailAsync(ReservationEmailModel model)
    {
        _localizer.SetCulture(model.PreferredLanguage);

        var subject = _localizer.T("Email.ReservationRejected.Subject");
        var htmlBody = BuildReservationRejectedEmail(model);

        await SendEmailAsync(model.ToEmail, subject, htmlBody);
    }

    private string BuildReservationConfirmedEmail(ReservationEmailModel model)
    {
        var greeting = _localizer.T("Email.Greeting", model.CustomerName);
        var message = _localizer.T("Email.ReservationConfirmed.Message");
        var details = _localizer.T("Email.ReservationDetails");
        var tourLabel = _localizer.T("Email.Tour");
        var companyLabel = _localizer.T("Email.Company");
        var destinationLabel = _localizer.T("Email.Destination");
        var dateLabel = _localizer.T("Email.Date");
        var peopleLabel = _localizer.T("Email.People");
        var totalLabel = _localizer.T("Email.Total");
        var footer = _localizer.T("Email.Footer");
        var thankYou = _localizer.T("Email.ThankYou");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .details table {{ width: 100%; }}
        .details td {{ padding: 8px 0; border-bottom: 1px solid #eee; }}
        .details td:first-child {{ font-weight: bold; width: 40%; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .success-icon {{ font-size: 48px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>&#10004;</div>
            <h1>{_localizer.T("Email.ReservationConfirmed.Title")}</h1>
        </div>
        <div class='content'>
            <p>{greeting}</p>
            <p>{message}</p>

            <div class='details'>
                <h3>{details}</h3>
                <table>
                    <tr><td>{tourLabel}:</td><td>{model.TourName}</td></tr>
                    <tr><td>{companyLabel}:</td><td>{model.CompanyName}</td></tr>
                    <tr><td>{destinationLabel}:</td><td>{model.Destination}</td></tr>
                    <tr><td>{dateLabel}:</td><td>{model.StartDate:dd.MM.yyyy} - {model.EndDate:dd.MM.yyyy}</td></tr>
                    <tr><td>{peopleLabel}:</td><td>{model.NumberOfPeople}</td></tr>
                    <tr><td><strong>{totalLabel}:</strong></td><td><strong>{model.TotalPrice:N2} TL</strong></td></tr>
                </table>
            </div>

            <p>{thankYou}</p>
        </div>
        <div class='footer'>
            <p>{footer}</p>
            <p>Erkan Tatil Plani &copy; {DateTime.Now.Year}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string BuildReservationCancelledEmail(ReservationEmailModel model)
    {
        var greeting = _localizer.T("Email.Greeting", model.CustomerName);
        var message = _localizer.T("Email.ReservationCancelled.Message");
        var details = _localizer.T("Email.ReservationDetails");
        var tourLabel = _localizer.T("Email.Tour");
        var companyLabel = _localizer.T("Email.Company");
        var footer = _localizer.T("Email.Footer");
        var contact = _localizer.T("Email.ContactUs");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .details table {{ width: 100%; }}
        .details td {{ padding: 8px 0; border-bottom: 1px solid #eee; }}
        .details td:first-child {{ font-weight: bold; width: 40%; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .cancel-icon {{ font-size: 48px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='cancel-icon'>&#10006;</div>
            <h1>{_localizer.T("Email.ReservationCancelled.Title")}</h1>
        </div>
        <div class='content'>
            <p>{greeting}</p>
            <p>{message}</p>

            <div class='details'>
                <h3>{details}</h3>
                <table>
                    <tr><td>{tourLabel}:</td><td>{model.TourName}</td></tr>
                    <tr><td>{companyLabel}:</td><td>{model.CompanyName}</td></tr>
                </table>
            </div>

            <p>{contact}</p>
        </div>
        <div class='footer'>
            <p>{footer}</p>
            <p>Erkan Tatil Plani &copy; {DateTime.Now.Year}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string BuildReservationRejectedEmail(ReservationEmailModel model)
    {
        var greeting = _localizer.T("Email.Greeting", model.CustomerName);
        var message = _localizer.T("Email.ReservationRejected.Message");
        var details = _localizer.T("Email.ReservationDetails");
        var tourLabel = _localizer.T("Email.Tour");
        var companyLabel = _localizer.T("Email.Company");
        var reasonLabel = _localizer.T("Email.Reason");
        var footer = _localizer.T("Email.Footer");
        var contact = _localizer.T("Email.ContactUs");

        var reasonSection = !string.IsNullOrEmpty(model.RejectionReason)
            ? $"<tr><td>{reasonLabel}:</td><td>{model.RejectionReason}</td></tr>"
            : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #ffc107; color: #333; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .details {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .details table {{ width: 100%; }}
        .details td {{ padding: 8px 0; border-bottom: 1px solid #eee; }}
        .details td:first-child {{ font-weight: bold; width: 40%; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning-icon {{ font-size: 48px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='warning-icon'>&#9888;</div>
            <h1>{_localizer.T("Email.ReservationRejected.Title")}</h1>
        </div>
        <div class='content'>
            <p>{greeting}</p>
            <p>{message}</p>

            <div class='details'>
                <h3>{details}</h3>
                <table>
                    <tr><td>{tourLabel}:</td><td>{model.TourName}</td></tr>
                    <tr><td>{companyLabel}:</td><td>{model.CompanyName}</td></tr>
                    {reasonSection}
                </table>
            </div>

            <p>{contact}</p>
        </div>
        <div class='footer'>
            <p>{footer}</p>
            <p>Erkan Tatil Plani &copy; {DateTime.Now.Year}</p>
        </div>
    </div>
</body>
</html>";
    }
}
