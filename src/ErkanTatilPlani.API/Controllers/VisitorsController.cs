using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VisitorsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Visitor>>> GetVisitors()
    {
        return await _context.Visitors.Where(v => v.IsActive).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Visitor>> GetVisitor(int id)
    {
        var visitor = await _context.Visitors.FindAsync(id);
        if (visitor == null) return NotFound();
        return visitor;
    }

    [HttpPost]
    public async Task<ActionResult<Visitor>> CreateVisitor(Visitor visitor)
    {
        _context.Visitors.Add(visitor);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVisitor), new { id = visitor.Id }, visitor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVisitor(int id, Visitor visitor)
    {
        if (id != visitor.Id) return BadRequest();
        visitor.UpdatedAt = DateTime.UtcNow;
        _context.Entry(visitor).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
