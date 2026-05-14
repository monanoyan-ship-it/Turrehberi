using System.Net;
using System.Net.Mail;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.EmailAccounts;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.EmailAccounts;

public class EmailAccountFactory : IEmailAccountFactory
{
    private readonly IEmailAccountEntityService _service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailAccountFactory> _logger;

    public EmailAccountFactory(IEmailAccountEntityService service, IUnitOfWork unitOfWork, ILogger<EmailAccountFactory> logger)
    {
        _service = service;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<object> GetAllAsync()
    {
        return await _service.GetActiveAccounts()
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new
            {
                a.Id, a.Name, a.Description, a.SmtpHost, a.SmtpPort, a.SmtpUsername,
                a.FromEmail, a.FromName, a.EnableSsl, a.IsDefault, a.DisplayOrder,
                a.IsActive, a.CreatedAt, a.UpdatedAt,
                TemplateCount = a.EmailTemplates.Count(t => t.IsActive)
            })
            .ToListAsync();
    }

    public async Task<object?> GetByIdAsync(int id)
    {
        return await _service.GetActiveAccounts()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id, a.Name, a.Description, a.SmtpHost, a.SmtpPort, a.SmtpUsername,
                a.FromEmail, a.FromName, a.EnableSsl, a.IsDefault, a.DisplayOrder,
                a.IsActive, a.CreatedAt, a.UpdatedAt,
                TemplateCount = a.EmailTemplates.Count(t => t.IsActive)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool success, object result, int statusCode)> CreateAsync(string name, string? description, string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, string fromEmail, string fromName, bool enableSsl, bool isDefault, int displayOrder)
    {
        if (await _service.NameExistsAsync(name))
            return (false, new { message = "Error.EmailAccountNameExists" }, 400);

        if (isDefault)
            await _service.ClearDefaultAsync();

        var account = new EmailAccount
        {
            Name = name, Description = description, SmtpHost = smtpHost, SmtpPort = smtpPort,
            SmtpUsername = smtpUsername, SmtpPassword = smtpPassword, FromEmail = fromEmail,
            FromName = fromName, EnableSsl = enableSsl, IsDefault = isDefault,
            DisplayOrder = displayOrder, CreatedAt = DateTime.UtcNow, IsActive = true
        };

        _service.Add(account);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email account created: {Name}", account.Name);

        return (true, new
        {
            account.Id, account.Name, account.Description, account.SmtpHost, account.SmtpPort,
            account.SmtpUsername, account.FromEmail, account.FromName, account.EnableSsl,
            account.IsDefault, account.DisplayOrder, account.IsActive, account.CreatedAt, TemplateCount = 0
        }, 201);
    }

    public async Task<(bool success, string message, int statusCode)> UpdateAsync(int id, string name, string? description, string smtpHost, int smtpPort, string smtpUsername, string? smtpPassword, string fromEmail, string fromName, bool enableSsl, int displayOrder)
    {
        var account = await _service.GetByIdAsync(id);
        if (account == null || !account.IsActive)
            return (false, "Error.EmailAccountNotFound", 404);

        if (await _service.NameExistsAsync(name, id))
            return (false, "Bu isimde bir email hesabi zaten mevcut", 400);

        account.Name = name;
        account.Description = description;
        account.SmtpHost = smtpHost;
        account.SmtpPort = smtpPort;
        account.SmtpUsername = smtpUsername;
        account.FromEmail = fromEmail;
        account.FromName = fromName;
        account.EnableSsl = enableSsl;
        account.DisplayOrder = displayOrder;
        account.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(smtpPassword))
            account.SmtpPassword = smtpPassword;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email account updated: {Name}", account.Name);
        return (true, "Success.EmailAccountUpdated", 200);
    }

    public async Task<(bool success, string message, int statusCode)> DeleteAsync(int id)
    {
        var account = await _service.GetByIdAsync(id);
        if (account == null || !account.IsActive)
            return (false, "Error.EmailAccountNotFound", 404);
        if (account.IsDefault)
            return (false, "Error.DefaultEmailAccountCannotBeDeleted", 400);

        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email account deleted: {Name}", account.Name);
        return (true, "Success.EmailAccountDeleted", 200);
    }

    public async Task<(bool success, string message, int statusCode)> SetDefaultAsync(int id)
    {
        var account = await _service.GetByIdAsync(id);
        if (account == null || !account.IsActive)
            return (false, "Error.EmailAccountNotFound", 404);

        await _service.ClearDefaultExceptAsync(id);
        account.IsDefault = true;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email account set as default: {Name}", account.Name);
        return (true, "Success.DefaultEmailAccountSet", 200);
    }

    public async Task<(bool success, object? result, int statusCode)> CopyAsync(int id)
    {
        var source = await _service.GetByIdAsync(id);
        if (source == null || !source.IsActive)
            return (false, new { message = "Error.EmailAccountNotFound" }, 404);

        var baseName = source.Name + " (Kopya)";
        var newName = baseName;
        var counter = 1;
        while (await _service.NameExistsAsync(newName))
            newName = $"{baseName} {counter++}";

        var copy = new EmailAccount
        {
            Name = newName, Description = source.Description, SmtpHost = source.SmtpHost,
            SmtpPort = source.SmtpPort, SmtpUsername = source.SmtpUsername, SmtpPassword = source.SmtpPassword,
            FromEmail = source.FromEmail, FromName = source.FromName, EnableSsl = source.EnableSsl,
            IsDefault = false, DisplayOrder = source.DisplayOrder + 1, CreatedAt = DateTime.UtcNow, IsActive = true
        };

        _service.Add(copy);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email account copied: {SourceName} -> {CopyName}", source.Name, copy.Name);

        return (true, new
        {
            copy.Id, copy.Name, copy.Description, copy.SmtpHost, copy.SmtpPort, copy.SmtpUsername,
            copy.FromEmail, copy.FromName, copy.EnableSsl, copy.IsDefault, copy.DisplayOrder,
            copy.IsActive, copy.CreatedAt, TemplateCount = 0
        }, 201);
    }

    public async Task<(bool success, string message, int statusCode)> TestAsync(int id, string toEmail)
    {
        var account = await _service.GetByIdAsync(id);
        if (account == null || !account.IsActive)
            return (false, "Error.EmailAccountNotFound", 404);

        if (string.IsNullOrWhiteSpace(toEmail))
            return (false, "Validation.RecipientEmailRequired", 400);

        try
        {
            using var client = new SmtpClient(account.SmtpHost, account.SmtpPort)
            {
                Credentials = new NetworkCredential(account.SmtpUsername, account.SmtpPassword),
                EnableSsl = account.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(account.FromEmail, account.FromName),
                Subject = "Tours - Test Email",
                Body = $@"<html><body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2>Test Email Basarili!</h2>
                    <p>Bu email, <strong>{account.Name}</strong> hesabindan gonderildi.</p>
                    <hr/><p><small>Tarih: {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss} UTC</small></p>
                    </body></html>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);
            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Test email sent from account {Name} to {ToEmail}", account.Name, toEmail);
            return (true, "Success.TestEmailSent", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email from account {Name} to {ToEmail}", account.Name, toEmail);
            return (false, "Error.EmailSendFailed", 400);
        }
    }
}
