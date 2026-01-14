using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public LanguagesController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private string GetLocalizationPath() => Path.Combine(_env.ContentRootPath, "Localization");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetLanguages()
    {
        var languages = await _context.Languages
            .Where(l => l.IsActive)
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.LanguageCulture,
                l.UniqueSeoCode,
                FlagIcon = l.FlagIcon ?? "fi fi-un",
                l.Rtl,
                l.IsDefault,
                l.DisplayOrder,
                l.IsActive,
                ResourceCount = l.LocaleStringResources.Count
            })
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();

        return Ok(languages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Language>> GetLanguage(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return NotFound();
        return language;
    }

    [HttpPost]
    public async Task<ActionResult<Language>> CreateLanguage(Language language)
    {
        _context.Languages.Add(language);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLanguage), new { id = language.Id }, language);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLanguage(int id, Language language)
    {
        if (id != language.Id) return BadRequest();
        language.UpdatedAt = DateTime.UtcNow;
        _context.Entry(language).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return NotFound();
        language.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return NotFound();

        // Remove default from all languages
        await _context.Languages
            .Where(l => l.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.IsDefault, false));

        // Set this language as default
        language.IsDefault = true;
        language.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/export")]
    public async Task<IActionResult> ExportXml(int id)
    {
        var language = await _context.Languages
            .Include(l => l.LocaleStringResources)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (language == null) return NotFound();

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
        return File(bytes, "application/xml", $"{language.UniqueSeoCode}.xml");
    }

    [HttpPost("{id}/import")]
    public async Task<IActionResult> ImportXml(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Dosya secilmedi" });

        var language = await _context.Languages.FindAsync(id);
        if (language == null) return NotFound();

        try
        {
            using var stream = file.OpenReadStream();
            var xml = await XDocument.LoadAsync(stream, LoadOptions.None, default);

            var resources = xml.Descendants("LocaleResource")
                .Select(x => new
                {
                    Name = x.Attribute("Name")?.Value ?? string.Empty,
                    Value = x.Value
                })
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToList();

            var existingResources = await _context.LocaleStringResources
                .Where(r => r.LanguageId == id)
                .ToDictionaryAsync(r => r.ResourceName, r => r);

            int importedCount = 0;
            int updatedCount = 0;

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
                    _context.LocaleStringResources.Add(new LocaleStringResource
                    {
                        LanguageId = id,
                        ResourceName = resource.Name,
                        ResourceValue = resource.Value,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    importedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                importedCount,
                updatedCount,
                totalCount = resources.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "XML dosyasi okunamadi: " + ex.Message });
        }
    }

    [HttpGet("{id}/resources")]
    public async Task<ActionResult<IEnumerable<LocaleStringResource>>> GetResources(int id)
    {
        var resources = await _context.LocaleStringResources
            .Where(r => r.LanguageId == id && r.IsActive)
            .OrderBy(r => r.ResourceName)
            .ToListAsync();

        return Ok(resources);
    }

    [HttpPut("{id}/resources/{resourceId}")]
    public async Task<IActionResult> UpdateResource(int id, int resourceId, [FromBody] LocaleStringResource resource)
    {
        if (resourceId != resource.Id || id != resource.LanguageId)
            return BadRequest();

        resource.UpdatedAt = DateTime.UtcNow;
        _context.Entry(resource).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Klasorden XML dosyasini yukler (culture code'a gore)
    /// </summary>
    [HttpPost("{id}/import-from-folder")]
    public async Task<IActionResult> ImportFromFolder(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return NotFound();

        var filePath = Path.Combine(GetLocalizationPath(), $"{language.LanguageCulture}.xml");
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = $"Dosya bulunamadi: {language.LanguageCulture}.xml" });

        try
        {
            var result = await ImportXmlFromFile(id, filePath);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "XML dosyasi okunamadi: " + ex.Message });
        }
    }

    /// <summary>
    /// Tum diller icin klasordeki XML dosyalarini yukler
    /// </summary>
    [HttpPost("import-all")]
    public async Task<IActionResult> ImportAll()
    {
        var languages = await _context.Languages.Where(l => l.IsActive).ToListAsync();
        var results = new List<object>();

        foreach (var language in languages)
        {
            var filePath = Path.Combine(GetLocalizationPath(), $"{language.LanguageCulture}.xml");
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    var result = await ImportXmlFromFile(language.Id, filePath);
                    results.Add(new
                    {
                        language = language.Name,
                        culture = language.LanguageCulture,
                        success = true,
                        result
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        language = language.Name,
                        culture = language.LanguageCulture,
                        success = false,
                        error = ex.Message
                    });
                }
            }
            else
            {
                results.Add(new
                {
                    language = language.Name,
                    culture = language.LanguageCulture,
                    success = false,
                    error = "Dosya bulunamadi"
                });
            }
        }

        return Ok(new { results, totalLanguages = languages.Count });
    }

    /// <summary>
    /// Klasordeki mevcut XML dosyalarini listeler
    /// </summary>
    [HttpGet("available-files")]
    public IActionResult GetAvailableFiles()
    {
        var localizationPath = GetLocalizationPath();
        if (!Directory.Exists(localizationPath))
            return Ok(new { files = Array.Empty<string>() });

        var files = Directory.GetFiles(localizationPath, "*.xml")
            .Select(f => Path.GetFileName(f))
            .ToList();

        return Ok(new { path = localizationPath, files });
    }

    private async Task<object> ImportXmlFromFile(int languageId, string filePath)
    {
        using var stream = System.IO.File.OpenRead(filePath);
        var xml = await XDocument.LoadAsync(stream, LoadOptions.None, default);

        var resources = xml.Descendants("LocaleResource")
            .Select(x => new
            {
                Name = x.Attribute("Name")?.Value ?? string.Empty,
                Value = x.Value
            })
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .ToList();

        var existingResources = await _context.LocaleStringResources
            .Where(r => r.LanguageId == languageId)
            .ToDictionaryAsync(r => r.ResourceName, r => r);

        int importedCount = 0;
        int updatedCount = 0;

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
                _context.LocaleStringResources.Add(new LocaleStringResource
                {
                    LanguageId = languageId,
                    ResourceName = resource.Name,
                    ResourceValue = resource.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                importedCount++;
            }
        }

        await _context.SaveChangesAsync();

        return new
        {
            importedCount,
            updatedCount,
            totalCount = resources.Count
        };
    }
}
