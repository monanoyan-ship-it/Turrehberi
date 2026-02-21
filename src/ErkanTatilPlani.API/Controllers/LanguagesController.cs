using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Languages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageFactory _languageFactory;

    public LanguagesController(ILanguageFactory languageFactory)
    {
        _languageFactory = languageFactory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetLanguages()
        => Ok(await _languageFactory.GetLanguagesAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Language>> GetLanguage(int id)
    {
        var language = await _languageFactory.GetLanguageAsync(id);
        if (language == null) return NotFound();
        return language;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Language>> CreateLanguage(Language language)
    {
        var created = await _languageFactory.CreateLanguageAsync(language);
        return CreatedAtAction(nameof(GetLanguage), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateLanguage(int id, Language language)
    {
        var (success, message, statusCode) = await _languageFactory.UpdateLanguageAsync(id, language);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        var (success, message, statusCode) = await _languageFactory.DeleteLanguageAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }

    [HttpPut("{id}/set-default")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var (success, message, statusCode) = await _languageFactory.SetDefaultAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }
}
