using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Languages;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageFactory _languageFactory;
    private readonly ILanguageResourceFactory _resourceFactory;

    public LanguagesController(ILanguageFactory languageFactory, ILanguageResourceFactory resourceFactory)
    {
        _languageFactory = languageFactory;
        _resourceFactory = resourceFactory;
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
    public async Task<ActionResult<Language>> CreateLanguage(Language language)
    {
        var created = await _languageFactory.CreateLanguageAsync(language);
        return CreatedAtAction(nameof(GetLanguage), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLanguage(int id, Language language)
    {
        var (success, message, statusCode) = await _languageFactory.UpdateLanguageAsync(id, language);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        var (success, message, statusCode) = await _languageFactory.DeleteLanguageAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }

    [HttpPut("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var (success, message, statusCode) = await _languageFactory.SetDefaultAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }

    [HttpGet("{id}/export")]
    public async Task<IActionResult> ExportXml(int id)
    {
        var (success, data, fileName, error, statusCode) = await _resourceFactory.ExportXmlAsync(id);
        if (!success) return StatusCode(statusCode, new { error });
        return File(data!, "application/xml", fileName);
    }

    [HttpPost("{id}/import")]
    public async Task<IActionResult> ImportXml(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Dosya secilmedi" });

        using var stream = file.OpenReadStream();
        var (success, result, statusCode) = await _resourceFactory.ImportXmlAsync(id, stream);
        return StatusCode(statusCode, result);
    }

    [HttpGet("{id}/resources")]
    public async Task<ActionResult<IEnumerable<LocaleStringResource>>> GetResources(int id)
        => Ok(await _resourceFactory.GetResourcesAsync(id));

    [HttpPut("{id}/resources/{resourceId}")]
    public async Task<IActionResult> UpdateResource(int id, int resourceId, [FromBody] LocaleStringResource resource)
    {
        var (success, message, statusCode) = await _resourceFactory.UpdateResourceAsync(id, resourceId, resource);
        if (!success) return StatusCode(statusCode, new { message });
        return NoContent();
    }

    [HttpPost("{id}/import-from-folder")]
    public async Task<IActionResult> ImportFromFolder(int id)
    {
        var (success, result, statusCode) = await _resourceFactory.ImportFromFolderAsync(id);
        return StatusCode(statusCode, result);
    }

    [HttpPost("import-all")]
    public async Task<IActionResult> ImportAll()
        => Ok(await _resourceFactory.ImportAllAsync());

    [HttpGet("available-files")]
    public IActionResult GetAvailableFiles()
        => Ok(_resourceFactory.GetAvailableFiles());
}
