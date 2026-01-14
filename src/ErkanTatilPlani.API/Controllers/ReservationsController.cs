using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
    {
        return await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return NotFound();
        return reservation;
    }

    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
    {
        if (id != reservation.Id) return BadRequest();
        reservation.UpdatedAt = DateTime.UtcNow;
        _context.Entry(reservation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();
        reservation.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
