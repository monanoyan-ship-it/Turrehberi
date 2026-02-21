using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ILanguageEntityService
{
    IQueryable<Language> GetActiveLanguages();
    Task<IEnumerable<Language>> GetAllActiveAsync();
    Task<Language?> GetByIdAsync(int id);
    void Add(Language language);
    void Update(Language language);
    Task ClearDefaultAsync();
}
