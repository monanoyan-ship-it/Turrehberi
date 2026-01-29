using ErkanTatilPlani.Core.Localization;
using ErkanTatilPlani.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _localization;
    private readonly ICacheService _cache;

    public LocalizationController(ILocalizationService localization, ICacheService cache)
    {
        _localization = localization;
        _cache = cache;
    }

    [HttpGet("languages")]
    [ResponseCache(Duration = 86400)] // 24 saat cache (dil listesi nadiren degisir)
    public IActionResult GetLanguages()
    {
        var languages = _cache.Get<IEnumerable<LanguageInfo>>(CacheKeys.Languages);
        if (languages == null)
        {
            languages = _localization.GetAvailableLanguages().ToList();
            _cache.Set(CacheKeys.Languages, languages, CacheDurations.VeryLong);
        }
        return Ok(languages);
    }

    [HttpGet("{culture}")]
    [ResponseCache(Duration = 3600)] // 1 saat cache
    public IActionResult GetLocale(string culture)
    {
        var cacheKey = string.Format(CacheKeys.LocaleByLang, culture);
        var strings = _cache.Get<Dictionary<string, string>>(cacheKey);

        if (strings == null)
        {
            strings = _localization.GetAllStrings(culture);
            if (strings.Count == 0)
                return NotFound(new { error = "Language not found" });

            _cache.Set(cacheKey, strings, CacheDurations.Long);
        }

        return Ok(strings);
    }

    [HttpGet("{culture}/{key}")]
    [ResponseCache(Duration = 3600)] // 1 saat cache
    public IActionResult GetString(string culture, string key)
    {
        _localization.SetCulture(culture);
        var value = _localization.T(key);
        return Ok(new { key, value });
    }
}
