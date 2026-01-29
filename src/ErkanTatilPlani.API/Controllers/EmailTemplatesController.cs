using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ErkanTatilPlani.API.Controllers;

[Route("api/admin/email-templates")]
[ApiController]
public class EmailTemplatesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmailTemplatesController> _logger;

    // Desteklenen diller
    private static readonly string[] SupportedLanguages = { "tr", "en", "ru", "de", "es", "fr", "ar", "fa", "pt" };

    public EmailTemplatesController(AppDbContext context, ILogger<EmailTemplatesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // DTO'lar
    public class EmailTemplateListDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? EmailAccountId { get; set; }
        public string? EmailAccountName { get; set; }
        public bool IsSystemTemplate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> AvailableLanguages { get; set; } = new();
    }

    public class EmailTemplateDetailDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? EmailAccountId { get; set; }
        public string? EmailAccountName { get; set; }
        public List<string> Placeholders { get; set; } = new();
        public bool IsSystemTemplate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<EmailTemplateTranslationDto> Translations { get; set; } = new();
    }

    public class EmailTemplateTranslationDto
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class EmailTemplateCreateDto
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? EmailAccountId { get; set; }
        public List<string>? Placeholders { get; set; }
    }

    public class EmailTemplateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? EmailAccountId { get; set; }
        public List<string>? Placeholders { get; set; }
    }

    public class TranslationUpdateDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class PreviewDto
    {
        public string LanguageCode { get; set; } = "tr";
        public Dictionary<string, string>? PlaceholderValues { get; set; }
    }

    public class PreviewResultDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    // GET: api/admin/email-templates
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTemplateListDto>>> GetAll()
    {
        var templates = await _context.EmailTemplates
            .Where(t => t.IsActive)
            .Include(t => t.EmailAccount)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .OrderBy(t => t.Key)
            .ToListAsync();

        var result = templates.Select(t => new EmailTemplateListDto
        {
            Id = t.Id,
            Key = t.Key,
            Name = t.Name,
            Description = t.Description,
            EmailAccountId = t.EmailAccountId,
            EmailAccountName = t.EmailAccount?.Name,
            IsSystemTemplate = t.IsSystemTemplate,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            AvailableLanguages = t.Translations.Select(tr => tr.LanguageCode).OrderBy(l => l).ToList()
        }).ToList();

        return Ok(result);
    }

    // GET: api/admin/email-templates/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailTemplateDetailDto>> GetById(int id)
    {
        var template = await _context.EmailTemplates
            .Where(t => t.Id == id && t.IsActive)
            .Include(t => t.EmailAccount)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .FirstOrDefaultAsync();

        if (template == null)
            return NotFound(new { message = "Email sablonu bulunamadi" });

        var placeholders = new List<string>();
        if (!string.IsNullOrEmpty(template.Placeholders))
        {
            try
            {
                placeholders = JsonSerializer.Deserialize<List<string>>(template.Placeholders) ?? new List<string>();
            }
            catch { }
        }

        return Ok(new EmailTemplateDetailDto
        {
            Id = template.Id,
            Key = template.Key,
            Name = template.Name,
            Description = template.Description,
            EmailAccountId = template.EmailAccountId,
            EmailAccountName = template.EmailAccount?.Name,
            Placeholders = placeholders,
            IsSystemTemplate = template.IsSystemTemplate,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Translations = template.Translations.Select(tr => new EmailTemplateTranslationDto
            {
                Id = tr.Id,
                LanguageCode = tr.LanguageCode,
                Subject = tr.Subject,
                Body = tr.Body,
                CreatedAt = tr.CreatedAt,
                UpdatedAt = tr.UpdatedAt
            }).OrderBy(tr => tr.LanguageCode).ToList()
        });
    }

    // GET: api/admin/email-templates/by-key/{key}
    [HttpGet("by-key/{key}")]
    public async Task<ActionResult<EmailTemplateDetailDto>> GetByKey(string key)
    {
        var template = await _context.EmailTemplates
            .Where(t => t.Key == key && t.IsActive)
            .Include(t => t.EmailAccount)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .FirstOrDefaultAsync();

        if (template == null)
            return NotFound(new { message = "Email sablonu bulunamadi" });

        var placeholders = new List<string>();
        if (!string.IsNullOrEmpty(template.Placeholders))
        {
            try
            {
                placeholders = JsonSerializer.Deserialize<List<string>>(template.Placeholders) ?? new List<string>();
            }
            catch { }
        }

        return Ok(new EmailTemplateDetailDto
        {
            Id = template.Id,
            Key = template.Key,
            Name = template.Name,
            Description = template.Description,
            EmailAccountId = template.EmailAccountId,
            EmailAccountName = template.EmailAccount?.Name,
            Placeholders = placeholders,
            IsSystemTemplate = template.IsSystemTemplate,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Translations = template.Translations.Select(tr => new EmailTemplateTranslationDto
            {
                Id = tr.Id,
                LanguageCode = tr.LanguageCode,
                Subject = tr.Subject,
                Body = tr.Body,
                CreatedAt = tr.CreatedAt,
                UpdatedAt = tr.UpdatedAt
            }).OrderBy(tr => tr.LanguageCode).ToList()
        });
    }

    // POST: api/admin/email-templates
    [HttpPost]
    public async Task<ActionResult<EmailTemplateDetailDto>> Create([FromBody] EmailTemplateCreateDto dto)
    {
        // Key benzersizlik kontrolu
        if (await _context.EmailTemplates.AnyAsync(t => t.Key == dto.Key && t.IsActive))
            return BadRequest(new { message = "Bu anahtara sahip bir sablon zaten mevcut" });

        // EmailAccount kontrolu
        if (dto.EmailAccountId.HasValue)
        {
            var accountExists = await _context.EmailAccounts.AnyAsync(a => a.Id == dto.EmailAccountId && a.IsActive);
            if (!accountExists)
                return BadRequest(new { message = "Belirtilen email hesabi bulunamadi" });
        }

        var template = new EmailTemplate
        {
            Key = dto.Key,
            Name = dto.Name,
            Description = dto.Description,
            EmailAccountId = dto.EmailAccountId,
            Placeholders = dto.Placeholders != null ? JsonSerializer.Serialize(dto.Placeholders) : null,
            IsSystemTemplate = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email template created: {Key}", template.Key);

        return CreatedAtAction(nameof(GetById), new { id = template.Id }, new EmailTemplateDetailDto
        {
            Id = template.Id,
            Key = template.Key,
            Name = template.Name,
            Description = template.Description,
            EmailAccountId = template.EmailAccountId,
            Placeholders = dto.Placeholders ?? new List<string>(),
            IsSystemTemplate = template.IsSystemTemplate,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            Translations = new List<EmailTemplateTranslationDto>()
        });
    }

    // PUT: api/admin/email-templates/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailTemplateUpdateDto dto)
    {
        var template = await _context.EmailTemplates.FindAsync(id);
        if (template == null || !template.IsActive)
            return NotFound(new { message = "Email sablonu bulunamadi" });

        // EmailAccount kontrolu
        if (dto.EmailAccountId.HasValue)
        {
            var accountExists = await _context.EmailAccounts.AnyAsync(a => a.Id == dto.EmailAccountId && a.IsActive);
            if (!accountExists)
                return BadRequest(new { message = "Belirtilen email hesabi bulunamadi" });
        }

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.EmailAccountId = dto.EmailAccountId;
        template.Placeholders = dto.Placeholders != null ? JsonSerializer.Serialize(dto.Placeholders) : template.Placeholders;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email template updated: {Key}", template.Key);

        return Ok(new { message = "Email sablonu guncellendi" });
    }

    // DELETE: api/admin/email-templates/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _context.EmailTemplates.FindAsync(id);
        if (template == null || !template.IsActive)
            return NotFound(new { message = "Email sablonu bulunamadi" });

        // Sistem sablonu silinemez
        if (template.IsSystemTemplate)
            return BadRequest(new { message = "Sistem sablonu silinemez" });

        // Soft delete
        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email template deleted: {Key}", template.Key);

        return Ok(new { message = "Email sablonu silindi" });
    }

    // PUT: api/admin/email-templates/5/translations/tr
    [HttpPut("{id}/translations/{languageCode}")]
    public async Task<IActionResult> UpdateTranslation(int id, string languageCode, [FromBody] TranslationUpdateDto dto)
    {
        // Dil kodu kontrolu
        languageCode = languageCode.ToLower();
        if (!SupportedLanguages.Contains(languageCode))
            return BadRequest(new { message = $"Desteklenmeyen dil kodu: {languageCode}. Desteklenen diller: {string.Join(", ", SupportedLanguages)}" });

        var template = await _context.EmailTemplates.FindAsync(id);
        if (template == null || !template.IsActive)
            return NotFound(new { message = "Email sablonu bulunamadi" });

        var translation = await _context.EmailTemplateTranslations
            .FirstOrDefaultAsync(t => t.EmailTemplateId == id && t.LanguageCode == languageCode);

        if (translation == null)
        {
            // Yeni ceviri olustur
            translation = new EmailTemplateTranslation
            {
                EmailTemplateId = id,
                LanguageCode = languageCode,
                Subject = dto.Subject,
                Body = dto.Body,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.EmailTemplateTranslations.Add(translation);
        }
        else
        {
            // Mevcut ceviriyi guncelle
            translation.Subject = dto.Subject;
            translation.Body = dto.Body;
            translation.UpdatedAt = DateTime.UtcNow;
            translation.IsActive = true;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email template translation updated: {Key} - {Lang}", template.Key, languageCode);

        return Ok(new { message = $"{languageCode.ToUpper()} cevirisi guncellendi" });
    }

    // DELETE: api/admin/email-templates/5/translations/tr
    [HttpDelete("{id}/translations/{languageCode}")]
    public async Task<IActionResult> DeleteTranslation(int id, string languageCode)
    {
        languageCode = languageCode.ToLower();

        var translation = await _context.EmailTemplateTranslations
            .FirstOrDefaultAsync(t => t.EmailTemplateId == id && t.LanguageCode == languageCode && t.IsActive);

        if (translation == null)
            return NotFound(new { message = "Ceviri bulunamadi" });

        // Soft delete
        translation.IsActive = false;
        translation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email template translation deleted: TemplateId={Id} - {Lang}", id, languageCode);

        return Ok(new { message = "Ceviri silindi" });
    }

    // POST: api/admin/email-templates/5/preview
    [HttpPost("{id}/preview")]
    public async Task<ActionResult<PreviewResultDto>> Preview(int id, [FromBody] PreviewDto dto)
    {
        var template = await _context.EmailTemplates
            .Where(t => t.Id == id && t.IsActive)
            .Include(t => t.Translations.Where(tr => tr.IsActive))
            .FirstOrDefaultAsync();

        if (template == null)
            return NotFound(new { message = "Email sablonu bulunamadi" });

        // Dil secimi: istenen dil -> tr -> en -> ilk bulunan
        var translation = template.Translations.FirstOrDefault(t => t.LanguageCode == dto.LanguageCode)
            ?? template.Translations.FirstOrDefault(t => t.LanguageCode == "tr")
            ?? template.Translations.FirstOrDefault(t => t.LanguageCode == "en")
            ?? template.Translations.FirstOrDefault();

        if (translation == null)
            return BadRequest(new { message = "Bu sablon icin hicbir ceviri bulunamadi" });

        var subject = translation.Subject;
        var body = translation.Body;

        // Placeholder'lari degistir
        if (dto.PlaceholderValues != null)
        {
            foreach (var kvp in dto.PlaceholderValues)
            {
                var placeholder = kvp.Key.StartsWith("{") ? kvp.Key : $"{{{kvp.Key}}}";
                subject = subject.Replace(placeholder, kvp.Value);
                body = body.Replace(placeholder, kvp.Value);
            }
        }

        // Kalan placeholder'lari ornek degerlerle degistir
        subject = ReplaceRemainingPlaceholders(subject);
        body = ReplaceRemainingPlaceholders(body);

        return Ok(new PreviewResultDto
        {
            Subject = subject,
            Body = body
        });
    }

    // GET: api/admin/email-templates/languages
    [HttpGet("languages")]
    public ActionResult<IEnumerable<object>> GetSupportedLanguages()
    {
        var languages = new[]
        {
            new { code = "tr", name = "Turkce", flag = "fi fi-tr" },
            new { code = "en", name = "English", flag = "fi fi-us" },
            new { code = "ru", name = "Русский", flag = "fi fi-ru" },
            new { code = "de", name = "Deutsch", flag = "fi fi-de" },
            new { code = "es", name = "Espanol", flag = "fi fi-es" },
            new { code = "fr", name = "Francais", flag = "fi fi-fr" },
            new { code = "ar", name = "العربية", flag = "fi fi-sa" },
            new { code = "fa", name = "فارسی", flag = "fi fi-ir" },
            new { code = "pt", name = "Portugues", flag = "fi fi-pt" }
        };

        return Ok(languages);
    }

    private static string ReplaceRemainingPlaceholders(string text)
    {
        // {placeholder} formatindaki placeholder'lari bul ve ornek degerlerle degistir
        var pattern = @"\{(\w+)\}";
        return Regex.Replace(text, pattern, match =>
        {
            var placeholder = match.Groups[1].Value.ToLower();
            return placeholder switch
            {
                "customername" => "John Doe",
                "reseturl" => "https://erkantatilplani.com/reset?token=xxx",
                "verifyurl" => "https://erkantatilplani.com/verify?token=xxx",
                "tourname" => "Efes Antik Kent Turu",
                "companyname" => "Ege Tur",
                "destination" => "Selcuk, Izmir",
                "startdate" => DateTime.UtcNow.ToString("dd.MM.yyyy"),
                "enddate" => DateTime.UtcNow.AddDays(2).ToString("dd.MM.yyyy"),
                "numberofpeople" => "2",
                "totalprice" => "1,500.00",
                "rejectionreason" => "Kapasite dolulugu",
                _ => $"[{placeholder}]"
            };
        });
    }
}
