using ErkanTatilPlani.Core.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _localization;

    public LocalizationController(ILocalizationService localization)
    {
        _localization = localization;
    }

    [HttpGet("languages")]
    public IActionResult GetLanguages()
    {
        var languages = _localization.GetAvailableLanguages();
        return Ok(languages);
    }

    [HttpGet("{culture}")]
    public IActionResult GetLocale(string culture)
    {
        var strings = _localization.GetAllStrings(culture);
        if (strings.Count == 0)
            return NotFound(new { error = "Language not found" });

        return Ok(strings);
    }

    [HttpGet("{culture}/{key}")]
    public IActionResult GetString(string culture, string key)
    {
        _localization.SetCulture(culture);
        var value = _localization.T(key);
        return Ok(new { key, value });
    }
}
