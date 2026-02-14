using ErkanTatilPlani.Core.Factories.EmailTemplates;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[Route("api/admin/email-templates")]
[ApiController]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateFactory _emailTemplateFactory;

    public EmailTemplatesController(IEmailTemplateFactory emailTemplateFactory)
    {
        _emailTemplateFactory = emailTemplateFactory;
    }

    // DTO'lar
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

    [HttpGet]
    public async Task<ActionResult> GetAll() => Ok(await _emailTemplateFactory.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var (success, result, statusCode) = await _emailTemplateFactory.GetByIdAsync(id);
        return StatusCode(statusCode, result);
    }

    [HttpGet("by-key/{key}")]
    public async Task<ActionResult> GetByKey(string key)
    {
        var (success, result, statusCode) = await _emailTemplateFactory.GetByKeyAsync(key);
        return StatusCode(statusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] EmailTemplateCreateDto dto)
    {
        var (success, result, statusCode) = await _emailTemplateFactory.CreateAsync(dto.Key, dto.Name, dto.Description, dto.EmailAccountId, dto.Placeholders);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailTemplateUpdateDto dto)
    {
        var (success, message, statusCode) = await _emailTemplateFactory.UpdateAsync(id, dto.Name, dto.Description, dto.EmailAccountId, dto.Placeholders);
        return StatusCode(statusCode, new { message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message, statusCode) = await _emailTemplateFactory.DeleteAsync(id);
        return StatusCode(statusCode, new { message });
    }

    [HttpPut("{id}/translations/{languageCode}")]
    public async Task<IActionResult> UpdateTranslation(int id, string languageCode, [FromBody] TranslationUpdateDto dto)
    {
        var (success, message, statusCode) = await _emailTemplateFactory.UpdateTranslationAsync(id, languageCode, dto.Subject, dto.Body);
        return StatusCode(statusCode, new { message });
    }

    [HttpDelete("{id}/translations/{languageCode}")]
    public async Task<IActionResult> DeleteTranslation(int id, string languageCode)
    {
        var (success, message, statusCode) = await _emailTemplateFactory.DeleteTranslationAsync(id, languageCode);
        return StatusCode(statusCode, new { message });
    }

    [HttpPost("{id}/preview")]
    public async Task<ActionResult> Preview(int id, [FromBody] PreviewDto dto)
    {
        var (success, result, statusCode) = await _emailTemplateFactory.PreviewAsync(id, dto.LanguageCode, dto.PlaceholderValues);
        return StatusCode(statusCode, result);
    }

    [HttpGet("languages")]
    public ActionResult GetSupportedLanguages() => Ok(_emailTemplateFactory.GetSupportedLanguages());
}
