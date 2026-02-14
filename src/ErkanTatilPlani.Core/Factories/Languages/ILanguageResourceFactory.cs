using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Languages;

public interface ILanguageResourceFactory
{
    Task<(bool success, byte[]? data, string? fileName, string? error, int statusCode)> ExportXmlAsync(int id);
    Task<(bool success, object? result, int statusCode)> ImportXmlAsync(int id, Stream fileStream);
    Task<IEnumerable<LocaleStringResource>> GetResourcesAsync(int languageId);
    Task<(bool success, string message, int statusCode)> UpdateResourceAsync(int languageId, int resourceId, LocaleStringResource resource);
    Task<(bool success, object? result, int statusCode)> ImportFromFolderAsync(int id);
    Task<object> ImportAllAsync();
    object GetAvailableFiles();
}
