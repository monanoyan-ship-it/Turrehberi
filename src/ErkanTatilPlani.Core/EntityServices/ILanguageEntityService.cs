using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ILanguageEntityService
{
    IQueryable<Language> GetActiveLanguages();
    Task<IEnumerable<Language>> GetAllActiveAsync();
    Task<Language?> GetByIdAsync(int id);
    Task<Language?> GetByIdWithResourcesAsync(int id);
    void Add(Language language);
    void Update(Language language);
    Task ClearDefaultAsync();
    // LocaleStringResource
    Task<Dictionary<string, LocaleStringResource>> GetResourcesByLanguageIdAsync(int languageId);
    IQueryable<LocaleStringResource> GetActiveResources(int languageId);
    void AddResource(LocaleStringResource resource);
    void UpdateResource(LocaleStringResource resource);
}
