using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly AppDbContext _context;

    public ToursController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tour>>> GetTours()
    {
        return await _context.Tours
            .Include(t => t.Company)
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<Tour>>> GetFeaturedTours()
    {
        return await _context.Tours
            .Include(t => t.Company)
            .Where(t => t.IsActive && t.IsFeatured)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tour>> GetTour(int id)
    {
        var tour = await _context.Tours
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tour == null) return NotFound();
        return tour;
    }

    [HttpPost]
    public async Task<ActionResult<Tour>> CreateTour(Tour tour)
    {
        _context.Tours.Add(tour);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTour), new { id = tour.Id }, tour);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTour(int id, Tour tour)
    {
        if (id != tour.Id) return BadRequest();
        tour.UpdatedAt = DateTime.UtcNow;
        _context.Entry(tour).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTour(int id)
    {
        var tour = await _context.Tours.FindAsync(id);
        if (tour == null) return NotFound();
        tour.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
