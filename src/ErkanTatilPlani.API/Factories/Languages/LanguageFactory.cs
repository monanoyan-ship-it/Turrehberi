using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Languages;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Languages;

public class LanguageFactory : ILanguageFactory
{
    private readonly ILanguageEntityService _service;
    private readonly IUnitOfWork _unitOfWork;

    public LanguageFactory(ILanguageEntityService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetLanguagesAsync()
    {
        return await _service.GetActiveLanguages()
            .Select(l => new
            {
                l.Id, l.Name, l.LanguageCulture, l.UniqueSeoCode,
                FlagIcon = l.FlagIcon ?? "fi fi-un",
                l.Rtl, l.IsDefault, l.DisplayOrder, l.IsActive
            })
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync<object>();
    }

    public async Task<Language?> GetLanguageAsync(int id)
        => await _service.GetByIdAsync(id);

    public async Task<Language> CreateLanguageAsync(Language language)
    {
        _service.Add(language);
        await _unitOfWork.SaveChangesAsync();
        return language;
    }

    public async Task<(bool success, string message, int statusCode)> UpdateLanguageAsync(int id, Language language)
    {
        if (id != language.Id)
            return (false, "Id uyusmazligi", 400);

        language.UpdatedAt = DateTime.UtcNow;
        _service.Update(language);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Guncellendi", 204);
    }

    public async Task<(bool success, string message, int statusCode)> DeleteLanguageAsync(int id)
    {
        var language = await _service.GetByIdAsync(id);
        if (language == null)
            return (false, "Dil bulunamadi", 404);

        language.IsActive = false;
        language.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, "Dil silindi", 204);
    }

    public async Task<(bool success, string message, int statusCode)> SetDefaultAsync(int id)
    {
        var language = await _service.GetByIdAsync(id);
        if (language == null)
            return (false, "Dil bulunamadi", 404);

        await _service.ClearDefaultAsync();

        language.IsDefault = true;
        language.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, "Varsayilan dil ayarlandi", 204);
    }
}
