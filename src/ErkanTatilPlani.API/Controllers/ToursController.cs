using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<ActionResult<Tour>> CreateTour(Tour tour)
    {
        // Firma durumu kontrolu
        var companyCheck = await CheckCompanyApprovalStatus();
        if (companyCheck != null) return companyCheck;

        _context.Tours.Add(tour);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTour), new { id = tour.Id }, tour);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTour(int id, Tour tour)
    {
        if (id != tour.Id) return BadRequest();

        // Firma durumu kontrolu
        var companyCheck = await CheckCompanyApprovalStatus();
        if (companyCheck != null) return companyCheck;

        tour.UpdatedAt = DateTime.UtcNow;
        _context.Entry(tour).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTour(int id)
    {
        // Firma durumu kontrolu
        var companyCheck = await CheckCompanyApprovalStatus();
        if (companyCheck != null) return companyCheck;

        var tour = await _context.Tours.FindAsync(id);
        if (tour == null) return NotFound();
        tour.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Firma sahibinin firma durumunu kontrol eder.
    /// Sadece Approved firmalar tur islemleri yapabilir.
    /// </summary>
    private async Task<ActionResult?> CheckCompanyApprovalStatus()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitor = await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == int.Parse(userIdClaim) && v.IsActive);

        if (visitor == null)
            return Unauthorized(new { message = "Kullanici bulunamadi" });

        if (visitor.Company == null)
            return StatusCode(403, new { message = "Firma sahibi degilsiniz", code = "NOT_COMPANY_OWNER" });

        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
        {
            var statusMessage = visitor.Company.StatusId switch
            {
                0 => new { message = "Basvurunuz inceleniyor. Onaylandiktan sonra tur ekleyebilirsiniz.", code = "COMPANY_PENDING" },
                2 => new { message = "Basvurunuz reddedildi.", code = "COMPANY_REJECTED" },
                3 => new { message = "Firmaniz askiya alindi.", code = "COMPANY_SUSPENDED" },
                _ => new { message = "Firma durumu gecersiz.", code = "COMPANY_INVALID_STATUS" }
            };
            return StatusCode(403, statusMessage);
        }

        return null; // Firma onaylı, devam edilebilir
    }
}
