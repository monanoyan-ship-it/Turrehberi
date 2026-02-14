using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IEmailTemplateEntityService
{
    IQueryable<EmailTemplate> GetActiveTemplates();
    Task<EmailTemplate?> GetByIdAsync(int id);
    Task<EmailTemplate?> GetByIdWithTranslationsAsync(int id);
    Task<EmailTemplate?> GetByKeyWithTranslationsAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    Task<bool> AccountExistsAsync(int accountId);
    void Add(EmailTemplate template);
    void Update(EmailTemplate template);
    // Translations
    Task<EmailTemplateTranslation?> GetTranslationAsync(int templateId, string languageCode);
    void AddTranslation(EmailTemplateTranslation translation);
}
