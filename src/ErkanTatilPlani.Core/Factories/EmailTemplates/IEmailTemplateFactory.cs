namespace ErkanTatilPlani.Core.Factories.EmailTemplates;

public interface IEmailTemplateFactory
{
    Task<object> GetAllAsync();
    Task<(bool success, object? result, int statusCode)> GetByIdAsync(int id);
    Task<(bool success, object? result, int statusCode)> GetByKeyAsync(string key);
    Task<(bool success, object result, int statusCode)> CreateAsync(string key, string name, string? description, int? emailAccountId, List<string>? placeholders);
    Task<(bool success, string message, int statusCode)> UpdateAsync(int id, string name, string? description, int? emailAccountId, List<string>? placeholders);
    Task<(bool success, string message, int statusCode)> DeleteAsync(int id);
    Task<(bool success, string message, int statusCode)> UpdateTranslationAsync(int id, string languageCode, string subject, string body);
    Task<(bool success, string message, int statusCode)> DeleteTranslationAsync(int id, string languageCode);
    Task<(bool success, object? result, int statusCode)> PreviewAsync(int id, string languageCode, Dictionary<string, string>? placeholderValues);
    object GetSupportedLanguages();
}
