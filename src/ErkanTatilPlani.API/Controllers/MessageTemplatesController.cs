using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageTemplatesController : ControllerBase
{
    private readonly IMessageTemplateFactory _templateFactory;

    public MessageTemplatesController(IMessageTemplateFactory templateFactory)
    {
        _templateFactory = templateFactory;
    }

    private int? GetCompanyId()
    {
        var claim = User.FindFirst("CompanyId")?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll()
    {
        var companyId = GetCompanyId();
        if (companyId == null) return BadRequest(new { message = "Firma bilgisi bulunamadi" });
        return Ok(await _templateFactory.GetAllAsync(companyId.Value));
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] TemplateRequest request)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return BadRequest(new { message = "Firma bilgisi bulunamadi" });

        var (success, message, data) = await _templateFactory.CreateAsync(
            companyId.Value, request.Title, request.Content, request.DisplayOrder);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TemplateRequest request)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return BadRequest(new { message = "Firma bilgisi bulunamadi" });

        var (success, message) = await _templateFactory.UpdateAsync(
            companyId.Value, id, request.Title, request.Content, request.DisplayOrder);

        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == null) return BadRequest(new { message = "Firma bilgisi bulunamadi" });

        var (success, message) = await _templateFactory.DeleteAsync(companyId.Value, id);

        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    [HttpPost("{id}/use")]
    public async Task<IActionResult> IncrementUsage(int id)
    {
        var (success, message) = await _templateFactory.IncrementUsageAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }
}

public class TemplateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
