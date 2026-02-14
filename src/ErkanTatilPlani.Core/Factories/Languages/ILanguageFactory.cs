using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Languages;

public interface ILanguageFactory
{
    Task<IEnumerable<object>> GetLanguagesAsync();
    Task<Language?> GetLanguageAsync(int id);
    Task<Language> CreateLanguageAsync(Language language);
    Task<(bool success, string message, int statusCode)> UpdateLanguageAsync(int id, Language language);
    Task<(bool success, string message, int statusCode)> DeleteLanguageAsync(int id);
    Task<(bool success, string message, int statusCode)> SetDefaultAsync(int id);
}
