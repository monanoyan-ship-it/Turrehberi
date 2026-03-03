using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportController : ControllerBase
{
    private readonly ISupportTicketFactory _factory;

    public SupportController(ISupportTicketFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
        => Ok(await _factory.GetAllAsync());

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<object>> GetStats()
        => Ok(await _factory.GetStatsAsync());

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<SupportTicket>> GetById(int id)
    {
        var ticket = await _factory.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        return ticket;
    }

    [HttpPost]
    public async Task<ActionResult<SupportTicket>> Create(SupportTicket ticket)
    {
        var visitorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (visitorId != null)
            ticket.VisitorId = int.Parse(visitorId);

        var created = await _factory.CreateAsync(ticket);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}/reply")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyRequest request)
    {
        var (success, message, statusCode) = await _factory.ReplyAsync(id, request.Reply);
        if (!success) return StatusCode(statusCode, new { message });
        return Ok(new { message });
    }

    [HttpPut("{id}/read")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var (success, message, statusCode) = await _factory.MarkReadAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message, statusCode) = await _factory.DeleteAsync(id);
        if (!success) return StatusCode(statusCode, new { message });
        return Ok(new { message });
    }

    public class ReplyRequest
    {
        public string Reply { get; set; } = string.Empty;
    }
}
