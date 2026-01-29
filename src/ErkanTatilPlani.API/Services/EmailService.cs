using System.Net;
using System.Net.Mail;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Localization;
using ErkanTatilPlani.Core.Services;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ErkanTatilPlani.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILocalizationService _localizer;
    private readonly ILogger<EmailService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;

    private const string AccountsCacheKey = "EmailAccounts";
    private const string TemplatesCacheKey = "EmailTemplates";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public EmailService(
        IOptions<EmailSettings> settings,
        ILocalizationService localizer,
        ILogger<EmailService> logger,
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache)
    {
        _settings = settings.Value;
        _localizer = localizer;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    /// <summary>
    /// Cache'i temizler (admin guncelleme sonrasi)
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove(AccountsCacheKey);
        _cache.Remove(TemplatesCacheKey);
        _logger.LogInformation("Email cache invalidated");
    }

    /// <summary>
    /// Tum email hesaplarini DB'den veya cache'den getirir
    /// </summary>
    private async Task<List<EmailAccount>> GetAccountsAsync()
    {
        if (_cache.TryGetValue(AccountsCacheKey, out List<EmailAccount>? accounts) && accounts != null)
        {
            return accounts;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        accounts = await context.EmailAccounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        _cache.Set(AccountsCacheKey, accounts, CacheDuration);
        return accounts;
    }

    /// <summary>
    /// Tum email sablonlarini DB'den veya cache'den getirir
    /// </summary>
    private async Task<List<EmailTemplate>> GetTemplatesAsync()
    {
        if (_cache.TryGetValue(TemplatesCacheKey, out List<EmailTemplate>? templates) && templates != null)
        {
            return templates;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        templates = await context.EmailTemplates
            .Where(t => t.IsActive)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .ToListAsync();

        _cache.Set(TemplatesCacheKey, templates, CacheDuration);
        return templates;
    }

    /// <summary>
    /// Hesap ismine gore email hesabi ayarlarini getirir
    /// Oncelik: DB -> appsettings fallback
    /// </summary>
    private async Task<EmailAccountSettings> GetAccountAsync(string? accountName = null)
    {
        var accounts = await GetAccountsAsync();

        // Varsayilan hesap ismi
        var name = accountName ?? "default";

        // DB'de ara
        var dbAccount = accounts.FirstOrDefault(a => a.Name == name)
            ?? accounts.FirstOrDefault(a => a.IsDefault)
            ?? accounts.FirstOrDefault();

        if (dbAccount != null)
        {
            return new EmailAccountSettings
            {
                SmtpHost = dbAccount.SmtpHost,
                SmtpPort = dbAccount.SmtpPort,
                SmtpUsername = dbAccount.SmtpUsername,
                SmtpPassword = dbAccount.SmtpPassword,
                FromEmail = dbAccount.FromEmail,
                FromName = dbAccount.FromName,
                EnableSsl = dbAccount.EnableSsl
            };
        }

        // DB'de yoksa appsettings'e fallback
        if (_settings.Accounts.TryGetValue(name, out var account))
        {
            return account;
        }

        if (_settings.Accounts.TryGetValue(_settings.DefaultAccount, out var defaultAccount))
        {
            _logger.LogWarning("Email account '{AccountName}' not found in DB or config, using default", name);
            return defaultAccount;
        }

        throw new InvalidOperationException($"No email accounts configured. Requested: '{name}'");
    }

    /// <summary>
    /// Template key'e gore sablonu ve ilgili ceviriyi getirir
    /// Dil fallback: istenen -> tr -> en -> ilk bulunan
    /// </summary>
    private async Task<(EmailTemplate template, EmailTemplateTranslation translation)?> GetTemplateWithTranslationAsync(string templateKey, string language)
    {
        var templates = await GetTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.Key == templateKey);

        if (template == null)
        {
            _logger.LogWarning("Email template not found: {TemplateKey}", templateKey);
            return null;
        }

        var translation = template.Translations.FirstOrDefault(t => t.LanguageCode == language)
            ?? template.Translations.FirstOrDefault(t => t.LanguageCode == "tr")
            ?? template.Translations.FirstOrDefault(t => t.LanguageCode == "en")
            ?? template.Translations.FirstOrDefault();

        if (translation == null)
        {
            _logger.LogWarning("No translation found for template: {TemplateKey}", templateKey);
            return null;
        }

        return (template, translation);
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        return SendEmailAsync(toEmail, subject, htmlBody, "default");
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string accountName)
    {
        try
        {
            var account = await GetAccountAsync(accountName);

            using var client = new SmtpClient(account.SmtpHost, account.SmtpPort)
            {
                Credentials = new NetworkCredential(account.SmtpUsername, account.SmtpPassword),
                EnableSsl = account.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(account.FromEmail, account.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email} using account '{Account}'", toEmail, accountName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} using account '{Account}'", toEmail, accountName);
            // Don't throw - email failure shouldn't break the main flow
        }
    }

    /// <summary>
    /// Template tabanli email gonderimi
    /// </summary>
    public async Task SendTemplateEmailAsync(string toEmail, string templateKey, string language, Dictionary<string, string> placeholders)
    {
        var result = await GetTemplateWithTranslationAsync(templateKey, language);
        if (result == null)
        {
            _logger.LogError("Cannot send template email, template not found: {TemplateKey}", templateKey);
            return;
        }

        var (template, translation) = result.Value;

        // Placeholder'lari degistir
        var subject = translation.Subject;
        var body = translation.Body;

        foreach (var kvp in placeholders)
        {
            var placeholder = kvp.Key.StartsWith("{") ? kvp.Key : $"{{{kvp.Key}}}";
            subject = subject.Replace(placeholder, kvp.Value);
            body = body.Replace(placeholder, kvp.Value);
        }

        // Template'e atanmis hesabi kullan, yoksa default
        var accountName = "default";
        if (template.EmailAccountId.HasValue)
        {
            var accounts = await GetAccountsAsync();
            var assignedAccount = accounts.FirstOrDefault(a => a.Id == template.EmailAccountId);
            if (assignedAccount != null)
            {
                accountName = assignedAccount.Name;
            }
        }

        await SendEmailAsync(toEmail, subject, body, accountName);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl, string customerName, string language)
    {
        // Once DB'den template dene
        var result = await GetTemplateWithTranslationAsync("password_reset", language);
        if (result != null)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "customerName", customerName },
                { "resetUrl", resetUrl }
            };
            await SendTemplateEmailAsync(toEmail, "password_reset", language, placeholders);
            return;
        }

        // DB'de yoksa eski yontemle devam et (geriye uyumluluk)
        _localizer.SetCulture(language);

        var subject = _localizer.T("Email.PasswordReset.Subject");
        var htmlBody = EmailTemplates.GetPasswordResetTemplate(
            customerName,
            resetUrl,
            _localizer.T("Email.Greeting"),
            _localizer.T("Email.PasswordReset.Title"),
            _localizer.T("Email.PasswordReset.Message"),
            _localizer.T("Email.PasswordReset.ButtonText"),
            _localizer.T("Email.PasswordReset.Expiry"),
            _localizer.T("Email.Footer")
        );

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendEmailVerificationAsync(string toEmail, string verifyUrl, string customerName, string language)
    {
        // Once DB'den template dene
        var result = await GetTemplateWithTranslationAsync("email_verification", language);
        if (result != null)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "customerName", customerName },
                { "verifyUrl", verifyUrl }
            };
            await SendTemplateEmailAsync(toEmail, "email_verification", language, placeholders);
            return;
        }

        // DB'de yoksa eski yontemle devam et
        _localizer.SetCulture(language);

        var subject = _localizer.T("Email.Verification.Subject");
        var htmlBody = EmailTemplates.GetEmailVerificationTemplate(
            customerName,
            verifyUrl,
            _localizer.T("Email.Greeting"),
            _localizer.T("Email.Verification.Title"),
            _localizer.T("Email.Verification.Message"),
            _localizer.T("Email.Verification.ButtonText"),
            _localizer.T("Email.Footer")
        );

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendReservationConfirmedEmailAsync(ReservationEmailModel model)
    {
        // Once DB'den template dene
        var result = await GetTemplateWithTranslationAsync("reservation_confirmed", model.PreferredLanguage);
        if (result != null)
        {
            var placeholders = GetReservationPlaceholders(model);
            await SendTemplateEmailAsync(model.ToEmail, "reservation_confirmed", model.PreferredLanguage, placeholders);
            return;
        }

        // DB'de yoksa eski yontemle devam et
        _localizer.SetCulture(model.PreferredLanguage);

        var subject = _localizer.T("Email.ReservationConfirmed.Subject");
        var htmlBody = EmailTemplates.GetReservationConfirmedTemplate(
            model,
            _localizer.T("Email.Greeting"),
            _localizer.T("Email.ReservationConfirmed.Title"),
            _localizer.T("Email.ReservationConfirmed.Message"),
            _localizer.T("Email.ReservationDetails"),
            _localizer.T("Email.Tour"),
            _localizer.T("Email.Company"),
            _localizer.T("Email.Destination"),
            _localizer.T("Email.Date"),
            _localizer.T("Email.People"),
            _localizer.T("Email.Total"),
            _localizer.T("Email.ThankYou"),
            _localizer.T("Email.Footer")
        );

        var accountName = await GetReservationAccountNameAsync();
        await SendEmailAsync(model.ToEmail, subject, htmlBody, accountName);
    }

    public async Task SendReservationCancelledEmailAsync(ReservationEmailModel model)
    {
        // Once DB'den template dene
        var result = await GetTemplateWithTranslationAsync("reservation_cancelled", model.PreferredLanguage);
        if (result != null)
        {
            var placeholders = GetReservationPlaceholders(model);
            await SendTemplateEmailAsync(model.ToEmail, "reservation_cancelled", model.PreferredLanguage, placeholders);
            return;
        }

        // DB'de yoksa eski yontemle devam et
        _localizer.SetCulture(model.PreferredLanguage);

        var subject = _localizer.T("Email.ReservationCancelled.Subject");
        var htmlBody = EmailTemplates.GetReservationCancelledTemplate(
            model,
            _localizer.T("Email.Greeting"),
            _localizer.T("Email.ReservationCancelled.Title"),
            _localizer.T("Email.ReservationCancelled.Message"),
            _localizer.T("Email.ReservationDetails"),
            _localizer.T("Email.Tour"),
            _localizer.T("Email.Company"),
            _localizer.T("Email.Destination"),
            _localizer.T("Email.Date"),
            _localizer.T("Email.People"),
            _localizer.T("Email.Total"),
            _localizer.T("Email.ContactUs"),
            _localizer.T("Email.Footer")
        );

        var accountName = await GetReservationAccountNameAsync();
        await SendEmailAsync(model.ToEmail, subject, htmlBody, accountName);
    }

    public async Task SendReservationRejectedEmailAsync(ReservationEmailModel model)
    {
        // Once DB'den template dene
        var result = await GetTemplateWithTranslationAsync("reservation_rejected", model.PreferredLanguage);
        if (result != null)
        {
            var placeholders = GetReservationPlaceholders(model);
            placeholders["rejectionReason"] = model.RejectionReason ?? "";
            await SendTemplateEmailAsync(model.ToEmail, "reservation_rejected", model.PreferredLanguage, placeholders);
            return;
        }

        // DB'de yoksa eski yontemle devam et
        _localizer.SetCulture(model.PreferredLanguage);

        var subject = _localizer.T("Email.ReservationRejected.Subject");
        var htmlBody = EmailTemplates.GetReservationRejectedTemplate(
            model,
            _localizer.T("Email.Greeting"),
            _localizer.T("Email.ReservationRejected.Title"),
            _localizer.T("Email.ReservationRejected.Message"),
            _localizer.T("Email.ReservationDetails"),
            _localizer.T("Email.Tour"),
            _localizer.T("Email.Company"),
            _localizer.T("Email.Destination"),
            _localizer.T("Email.Date"),
            _localizer.T("Email.People"),
            _localizer.T("Email.Total"),
            _localizer.T("Email.Reason"),
            _localizer.T("Email.ContactUs"),
            _localizer.T("Email.Footer")
        );

        var accountName = await GetReservationAccountNameAsync();
        await SendEmailAsync(model.ToEmail, subject, htmlBody, accountName);
    }

    private Dictionary<string, string> GetReservationPlaceholders(ReservationEmailModel model)
    {
        return new Dictionary<string, string>
        {
            { "customerName", model.CustomerName },
            { "tourName", model.TourName },
            { "companyName", model.CompanyName },
            { "destination", model.Destination },
            { "startDate", model.StartDate.ToString("dd.MM.yyyy") },
            { "endDate", model.EndDate.ToString("dd.MM.yyyy") },
            { "numberOfPeople", model.NumberOfPeople.ToString() },
            { "totalPrice", model.TotalPrice.ToString("N2") }
        };
    }

    private async Task<string> GetReservationAccountNameAsync()
    {
        var accounts = await GetAccountsAsync();
        var reservationAccount = accounts.FirstOrDefault(a => a.Name == "reservations");
        return reservationAccount?.Name ?? "default";
    }
}
