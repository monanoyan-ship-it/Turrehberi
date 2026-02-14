using ErkanTatilPlani.Core.Factories.EmailAccounts;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[Route("api/admin/email-accounts")]
[ApiController]
public class EmailAccountsController : ControllerBase
{
    private readonly IEmailAccountFactory _emailAccountFactory;

    public EmailAccountsController(IEmailAccountFactory emailAccountFactory)
    {
        _emailAccountFactory = emailAccountFactory;
    }

    // DTO'lar
    public class EmailAccountCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class EmailAccountUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string? SmtpPassword { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class TestEmailDto
    {
        public string ToEmail { get; set; } = string.Empty;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll() => Ok(await _emailAccountFactory.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var result = await _emailAccountFactory.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "Email hesabi bulunamadi" });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] EmailAccountCreateDto dto)
    {
        var (success, result, statusCode) = await _emailAccountFactory.CreateAsync(dto.Name, dto.Description, dto.SmtpHost, dto.SmtpPort, dto.SmtpUsername, dto.SmtpPassword, dto.FromEmail, dto.FromName, dto.EnableSsl, dto.IsDefault, dto.DisplayOrder);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailAccountUpdateDto dto)
    {
        var (success, message, statusCode) = await _emailAccountFactory.UpdateAsync(id, dto.Name, dto.Description, dto.SmtpHost, dto.SmtpPort, dto.SmtpUsername, dto.SmtpPassword, dto.FromEmail, dto.FromName, dto.EnableSsl, dto.DisplayOrder);
        return StatusCode(statusCode, new { message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message, statusCode) = await _emailAccountFactory.DeleteAsync(id);
        return StatusCode(statusCode, new { message });
    }

    [HttpPut("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        var (success, message, statusCode) = await _emailAccountFactory.SetDefaultAsync(id);
        return StatusCode(statusCode, new { message });
    }

    [HttpPost("{id}/copy")]
    public async Task<ActionResult> Copy(int id)
    {
        var (success, result, statusCode) = await _emailAccountFactory.CopyAsync(id);
        return StatusCode(statusCode, result);
    }

    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestEmail(int id, [FromBody] TestEmailDto dto)
    {
        var (success, message, statusCode) = await _emailAccountFactory.TestAsync(id, dto.ToEmail);
        return StatusCode(statusCode, new { message });
    }
}
