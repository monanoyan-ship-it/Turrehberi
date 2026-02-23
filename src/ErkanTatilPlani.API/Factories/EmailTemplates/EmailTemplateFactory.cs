using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.EmailTemplates;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ErkanTatilPlani.API.Factories.EmailTemplates;

public class EmailTemplateFactory : IEmailTemplateFactory
{
    private static readonly string[] SupportedLanguages = { "tr", "en", "ru", "de", "es", "fr", "ar", "fa", "pt" };

    private readonly IEmailTemplateEntityService _service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailTemplateFactory> _logger;

    public EmailTemplateFactory(IEmailTemplateEntityService service, IUnitOfWork unitOfWork, ILogger<EmailTemplateFactory> logger)
    {
        _service = service;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<object> GetAllAsync()
    {
        return await _service.GetActiveTemplates()
            .Include(t => t.EmailAccount)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .OrderBy(t => t.Key)
            .Select(t => new
            {
                t.Id, t.Key, t.Name, t.Description, t.EmailAccountId,
                EmailAccountName = t.EmailAccount != null ? t.EmailAccount.Name : null,
                t.IsSystemTemplate, t.IsActive, t.CreatedAt, t.UpdatedAt,
                AvailableLanguages = t.Translations.Select(tr => tr.LanguageCode).OrderBy(l => l).ToList()
            })
            .ToListAsync();
    }

    public async Task<(bool success, object? result, int statusCode)> GetByIdAsync(int id)
    {
        var template = await _service.GetByIdWithTranslationsAsync(id);
        if (template == null)
            return (false, new { message = "Error.EmailTemplateNotFound" }, 404);

        return (true, MapTemplateDetail(template), 200);
    }

    public async Task<(bool success, object? result, int statusCode)> GetByKeyAsync(string key)
    {
        var template = await _service.GetByKeyWithTranslationsAsync(key);
        if (template == null)
            return (false, new { message = "Error.EmailTemplateNotFound" }, 404);

        return (true, MapTemplateDetail(template), 200);
    }

    public async Task<(bool success, object result, int statusCode)> CreateAsync(string key, string name, string? description, int? emailAccountId, List<string>? placeholders)
    {
        if (await _service.KeyExistsAsync(key))
            return (false, new { message = "Error.TemplateKeyExists" }, 400);

        if (emailAccountId.HasValue)
        {
            if (!await _service.AccountExistsAsync(emailAccountId.Value))
                return (false, new { message = "Error.SpecifiedEmailAccountNotFound" }, 400);
        }

        var template = new EmailTemplate
        {
            Key = key,
            Name = name,
            Description = description,
            EmailAccountId = emailAccountId,
            Placeholders = placeholders != null ? JsonSerializer.Serialize(placeholders) : null,
            IsSystemTemplate = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _service.Add(template);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email template created: {Key}", template.Key);

        return (true, new
        {
            template.Id, template.Key, template.Name, template.Description,
            template.EmailAccountId,
            Placeholders = placeholders ?? new List<string>(),
            template.IsSystemTemplate, template.IsActive, template.CreatedAt,
            Translations = new List<object>()
        }, 201);
    }

    public async Task<(bool success, string message, int statusCode)> UpdateAsync(int id, string name, string? description, int? emailAccountId, List<string>? placeholders)
    {
        var template = await _service.GetByIdAsync(id);
        if (template == null || !template.IsActive)
            return (false, "Error.EmailTemplateNotFound", 404);

        if (emailAccountId.HasValue)
        {
            if (!await _service.AccountExistsAsync(emailAccountId.Value))
                return (false, "Error.SpecifiedEmailAccountNotFound", 400);
        }

        template.Name = name;
        template.Description = description;
        template.EmailAccountId = emailAccountId;
        template.Placeholders = placeholders != null ? JsonSerializer.Serialize(placeholders) : template.Placeholders;
        template.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email template updated: {Key}", template.Key);

        return (true, "Success.EmailTemplateUpdated", 200);
    }

    public async Task<(bool success, string message, int statusCode)> DeleteAsync(int id)
    {
        var template = await _service.GetByIdAsync(id);
        if (template == null || !template.IsActive)
            return (false, "Error.EmailTemplateNotFound", 404);

        if (template.IsSystemTemplate)
            return (false, "Error.SystemTemplateCannotBeDeleted", 400);

        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email template deleted: {Key}", template.Key);

        return (true, "Success.EmailTemplateDeleted", 200);
    }

    public async Task<(bool success, string message, int statusCode)> UpdateTranslationAsync(int id, string languageCode, string subject, string body)
    {
        languageCode = languageCode.ToLower();
        if (!SupportedLanguages.Contains(languageCode))
            return (false, "Error.UnsupportedLanguageCode", 400);

        var template = await _service.GetByIdAsync(id);
        if (template == null || !template.IsActive)
            return (false, "Error.EmailTemplateNotFound", 404);

        var translation = await _service.GetTranslationAsync(id, languageCode);

        if (translation == null)
        {
            translation = new EmailTemplateTranslation
            {
                EmailTemplateId = id,
                LanguageCode = languageCode,
                Subject = subject,
                Body = body,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _service.AddTranslation(translation);
        }
        else
        {
            translation.Subject = subject;
            translation.Body = body;
            translation.UpdatedAt = DateTime.UtcNow;
            translation.IsActive = true;
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email template translation updated: {Key} - {Lang}", template.Key, languageCode);

        return (true, "Success.TranslationUpdated", 200);
    }

    public async Task<(bool success, string message, int statusCode)> DeleteTranslationAsync(int id, string languageCode)
    {
        languageCode = languageCode.ToLower();

        var translation = await _service.GetTranslationAsync(id, languageCode);
        if (translation == null || !translation.IsActive)
            return (false, "Error.TranslationNotFound", 404);

        translation.IsActive = false;
        translation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Email template translation deleted: TemplateId={Id} - {Lang}", id, languageCode);

        return (true, "Success.TranslationDeleted", 200);
    }

    public async Task<(bool success, object? result, int statusCode)> PreviewAsync(int id, string languageCode, Dictionary<string, string>? placeholderValues)
    {
        var template = await _service.GetByIdWithTranslationsAsync(id);
        if (template == null)
            return (false, new { message = "Error.EmailTemplateNotFound" }, 404);

        var activeTranslations = template.Translations.Where(t => t.IsActive).ToList();

        var translation = activeTranslations.FirstOrDefault(t => t.LanguageCode == languageCode)
            ?? activeTranslations.FirstOrDefault(t => t.LanguageCode == "tr")
            ?? activeTranslations.FirstOrDefault(t => t.LanguageCode == "en")
            ?? activeTranslations.FirstOrDefault();

        if (translation == null)
            return (false, new { message = "Error.NoTranslationsFound" }, 400);

        var subject = translation.Subject;
        var body = translation.Body;

        if (placeholderValues != null)
        {
            foreach (var kvp in placeholderValues)
            {
                var placeholder = kvp.Key.StartsWith("{") ? kvp.Key : $"{{{kvp.Key}}}";
                subject = subject.Replace(placeholder, kvp.Value);
                body = body.Replace(placeholder, kvp.Value);
            }
        }

        subject = ReplaceRemainingPlaceholders(subject);
        body = ReplaceRemainingPlaceholders(body);

        return (true, new { Subject = subject, Body = body }, 200);
    }

    public object GetSupportedLanguages()
    {
        return new[]
        {
            new { code = "tr", name = "Turkce", flag = "fi fi-tr" },
            new { code = "en", name = "English", flag = "fi fi-us" },
            new { code = "ru", name = "Русский", flag = "fi fi-ru" },
            new { code = "de", name = "Deutsch", flag = "fi fi-de" },
            new { code = "es", name = "Espanol", flag = "fi fi-es" },
            new { code = "fr", name = "Francais", flag = "fi fi-fr" },
            new { code = "ar", name = "العربية", flag = "fi fi-sa" },
            new { code = "fa", name = "فارسی", flag = "fi fi-ir" },
            new { code = "pt", name = "Portugues", flag = "fi fi-pt" }
        };
    }

    private static object MapTemplateDetail(EmailTemplate template)
    {
        var placeholders = new List<string>();
        if (!string.IsNullOrEmpty(template.Placeholders))
        {
            try { placeholders = JsonSerializer.Deserialize<List<string>>(template.Placeholders) ?? new List<string>(); }
            catch { }
        }

        return new
        {
            template.Id, template.Key, template.Name, template.Description,
            template.EmailAccountId,
            EmailAccountName = template.EmailAccount?.Name,
            Placeholders = placeholders,
            template.IsSystemTemplate, template.IsActive, template.CreatedAt, template.UpdatedAt,
            Translations = template.Translations
                .Where(tr => tr.IsActive)
                .Select(tr => new { tr.Id, tr.LanguageCode, tr.Subject, tr.Body, tr.CreatedAt, tr.UpdatedAt })
                .OrderBy(tr => tr.LanguageCode)
                .ToList()
        };
    }

    private static string ReplaceRemainingPlaceholders(string text)
    {
        var pattern = @"\{(\w+)\}";
        return Regex.Replace(text, pattern, match =>
        {
            var placeholder = match.Groups[1].Value.ToLower();
            return placeholder switch
            {
                "customername" => "John Doe",
                "reseturl" => "https://erkantatilplani.com/reset?token=xxx",
                "verifyurl" => "https://erkantatilplani.com/verify?token=xxx",
                "tourname" => "Efes Antik Kent Turu",
                "companyname" => "Ege Tur",
                "destination" => "Selcuk, Izmir",
                "startdate" => DateTime.UtcNow.ToString("dd.MM.yyyy"),
                "enddate" => DateTime.UtcNow.AddDays(2).ToString("dd.MM.yyyy"),
                "numberofpeople" => "2",
                "totalprice" => "1,500.00",
                "rejectionreason" => "Kapasite dolulugu",
                _ => $"[{placeholder}]"
            };
        });
    }
}
