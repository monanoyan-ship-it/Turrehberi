using System.Text;
using System.Xml.Linq;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Languages;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Languages;

public class LanguageResourceFactory : ILanguageResourceFactory
{
    private readonly ILanguageEntityService _service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public LanguageResourceFactory(ILanguageEntityService service, IUnitOfWork unitOfWork, IWebHostEnvironment env)
    {
        _service = service;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    public async Task<(bool success, byte[]? data, string? fileName, string? error, int statusCode)> ExportXmlAsync(int id)
    {
        var language = await _service.GetByIdWithResourcesAsync(id);
        if (language == null)
            return (false, null, null, "Dil bulunamadi", 404);

        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Language",
                new XAttribute("Name", language.Name),
                new XAttribute("Culture", language.LanguageCulture),
                language.LocaleStringResources
                    .OrderBy(r => r.ResourceName)
                    .Select(r => new XElement("LocaleResource",
                        new XAttribute("Name", r.ResourceName),
                        r.ResourceValue
                    ))
            )
        );

        var bytes = Encoding.UTF8.GetBytes(xml.ToString());
        return (true, bytes, $"{language.UniqueSeoCode}.xml", null, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> ImportXmlAsync(int id, Stream fileStream)
    {
        var language = await _service.GetByIdAsync(id);
        if (language == null)
            return (false, new { error = "Dil bulunamadi" }, 404);

        try
        {
            var xml = await XDocument.LoadAsync(fileStream, LoadOptions.None, default);
            var result = await ImportFromXDocument(id, xml);
            return (true, result, 200);
        }
        catch (Exception ex)
        {
            return (false, new { error = "XML dosyasi okunamadi: " + ex.Message }, 400);
        }
    }

    public async Task<IEnumerable<LocaleStringResource>> GetResourcesAsync(int languageId)
    {
        return await _service.GetActiveResources(languageId)
            .OrderBy(r => r.ResourceName)
            .ToListAsync();
    }

    public async Task<(bool success, string message, int statusCode)> UpdateResourceAsync(int languageId, int resourceId, LocaleStringResource resource)
    {
        if (resourceId != resource.Id || languageId != resource.LanguageId)
            return (false, "Id uyusmazligi", 400);

        resource.UpdatedAt = DateTime.UtcNow;
        _service.UpdateResource(resource);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Guncellendi", 204);
    }

    public async Task<(bool success, object? result, int statusCode)> ImportFromFolderAsync(int id)
    {
        var language = await _service.GetByIdAsync(id);
        if (language == null)
            return (false, new { error = "Dil bulunamadi" }, 404);

        var filePath = Path.Combine(GetLocalizationPath(), $"{language.LanguageCulture}.xml");
        if (!File.Exists(filePath))
            return (false, new { error = $"Dosya bulunamadi: {language.LanguageCulture}.xml" }, 404);

        try
        {
            var result = await ImportXmlFromFile(id, filePath);
            return (true, result, 200);
        }
        catch (Exception ex)
        {
            return (false, new { error = "XML dosyasi okunamadi: " + ex.Message }, 400);
        }
    }

    public async Task<object> ImportAllAsync()
    {
        var languages = await _service.GetAllActiveAsync();
        var results = new List<object>();

        foreach (var language in languages)
        {
            var filePath = Path.Combine(GetLocalizationPath(), $"{language.LanguageCulture}.xml");
            if (File.Exists(filePath))
            {
                try
                {
                    var result = await ImportXmlFromFile(language.Id, filePath);
                    results.Add(new { language = language.Name, culture = language.LanguageCulture, success = true, result });
                }
                catch (Exception ex)
                {
                    results.Add(new { language = language.Name, culture = language.LanguageCulture, success = false, error = ex.Message });
                }
            }
            else
            {
                results.Add(new { language = language.Name, culture = language.LanguageCulture, success = false, error = "Dosya bulunamadi" });
            }
        }

        return new { results, totalLanguages = results.Count };
    }

    public object GetAvailableFiles()
    {
        var localizationPath = GetLocalizationPath();
        if (!Directory.Exists(localizationPath))
            return new { files = Array.Empty<string>() };

        var files = Directory.GetFiles(localizationPath, "*.xml")
            .Select(f => Path.GetFileName(f))
            .ToList();

        return new { path = localizationPath, files };
    }

    private string GetLocalizationPath() => Path.Combine(_env.ContentRootPath, "Localization");

    private async Task<object> ImportFromXDocument(int languageId, XDocument xml)
    {
        var resources = xml.Descendants("LocaleResource")
            .Select(x => new { Name = x.Attribute("Name")?.Value ?? string.Empty, Value = x.Value })
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .ToList();

        var existingResources = await _service.GetResourcesByLanguageIdAsync(languageId);
        int importedCount = 0, updatedCount = 0;

        foreach (var resource in resources)
        {
            if (existingResources.TryGetValue(resource.Name, out var existing))
            {
                if (existing.ResourceValue != resource.Value)
                {
                    existing.ResourceValue = resource.Value;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updatedCount++;
                }
            }
            else
            {
                _service.AddResource(new LocaleStringResource
                {
                    LanguageId = languageId, ResourceName = resource.Name, ResourceValue = resource.Value,
                    CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true
                });
                importedCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return new { importedCount, updatedCount, totalCount = resources.Count };
    }

    private async Task<object> ImportXmlFromFile(int languageId, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var xml = await XDocument.LoadAsync(stream, LoadOptions.None, default);
        return await ImportFromXDocument(languageId, xml);
    }
}
