using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Visitors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitorsController : ControllerBase
{
    private readonly IVisitorFactory _visitorFactory;

    public VisitorsController(IVisitorFactory visitorFactory)
    {
        _visitorFactory = visitorFactory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Visitor>>> GetVisitors()
    {
        return Ok(await _visitorFactory.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Visitor>> GetVisitor(int id)
    {
        var visitor = await _visitorFactory.GetByIdAsync(id);
        if (visitor == null) return NotFound();
        return visitor;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Visitor>> CreateVisitor(Visitor visitor)
    {
        var created = await _visitorFactory.CreateAsync(visitor);
        return CreatedAtAction(nameof(GetVisitor), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateVisitor(int id, Visitor visitor)
    {
        if (id != visitor.Id) return BadRequest();
        await _visitorFactory.UpdateAsync(id, visitor);
        return NoContent();
    }
}
