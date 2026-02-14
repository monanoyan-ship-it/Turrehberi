using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class EmailTemplateEntityService : IEmailTemplateEntityService
{
    private readonly AppDbContext _context;

    public EmailTemplateEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<EmailTemplate> GetActiveTemplates()
        => _context.EmailTemplates.Where(t => t.IsActive);

    public async Task<EmailTemplate?> GetByIdAsync(int id)
        => await _context.EmailTemplates.FindAsync(id);

    public async Task<EmailTemplate?> GetByIdWithTranslationsAsync(int id)
        => await _context.EmailTemplates
            .Where(t => t.Id == id && t.IsActive)
            .Include(t => t.EmailAccount)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .FirstOrDefaultAsync();

    public async Task<EmailTemplate?> GetByKeyWithTranslationsAsync(string key)
        => await _context.EmailTemplates
            .Where(t => t.Key == key && t.IsActive)
            .Include(t => t.EmailAccount)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .FirstOrDefaultAsync();

    public async Task<bool> KeyExistsAsync(string key)
        => await _context.EmailTemplates.AnyAsync(t => t.Key == key && t.IsActive);

    public async Task<bool> AccountExistsAsync(int accountId)
        => await _context.EmailAccounts.AnyAsync(a => a.Id == accountId && a.IsActive);

    public void Add(EmailTemplate template) => _context.EmailTemplates.Add(template);

    public void Update(EmailTemplate template) => _context.Entry(template).State = EntityState.Modified;

    // Translations
    public async Task<EmailTemplateTranslation?> GetTranslationAsync(int templateId, string languageCode)
        => await _context.EmailTemplateTranslations.FirstOrDefaultAsync(t => t.EmailTemplateId == templateId && t.LanguageCode == languageCode);

    public void AddTranslation(EmailTemplateTranslation translation) => _context.EmailTemplateTranslations.Add(translation);
}
