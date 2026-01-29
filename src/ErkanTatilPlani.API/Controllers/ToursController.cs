using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Services;
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
    private readonly ICacheService _cache;

    public ToursController(AppDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetTours(
        [FromQuery] string? search = null,
        [FromQuery] string? destination = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minDays = null,
        [FromQuery] int? maxDays = null,
        [FromQuery] int? companyId = null,
        [FromQuery] bool? featured = null,
        [FromQuery] string? sort = null)
    {
        var query = _context.Tours
            .Include(t => t.Company)
            .Where(t => t.IsActive && t.Company!.StatusId == CompanyStatuses.Ids.Approved);

        // Arama (isim veya aciklama)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(searchLower) ||
                                     t.Description.ToLower().Contains(searchLower) ||
                                     t.Destination.ToLower().Contains(searchLower));
        }

        // Destinasyon filtresi
        if (!string.IsNullOrWhiteSpace(destination))
        {
            var destLower = destination.ToLower();
            query = query.Where(t => t.Destination.ToLower().Contains(destLower));
        }

        // Fiyat araligi
        if (minPrice.HasValue)
            query = query.Where(t => t.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(t => t.Price <= maxPrice.Value);

        // Sure araligi
        if (minDays.HasValue)
            query = query.Where(t => t.DurationDays >= minDays.Value);
        if (maxDays.HasValue)
            query = query.Where(t => t.DurationDays <= maxDays.Value);

        // Firma filtresi
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId.Value);

        // One cikan filtresi
        if (featured.HasValue && featured.Value)
            query = query.Where(t => t.IsFeatured);

        // Siralama
        query = sort?.ToLower() switch
        {
            "price_asc" => query.OrderBy(t => t.Price),
            "price_desc" => query.OrderByDescending(t => t.Price),
            "duration_asc" => query.OrderBy(t => t.DurationDays),
            "duration_desc" => query.OrderByDescending(t => t.DurationDays),
            "rating" => query.OrderByDescending(t => t.AverageRating),
            "newest" => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.IsFeatured).ThenByDescending(t => t.AverageRating)
        };

        var tours = await query.ToListAsync();

        // Benzersiz destinasyonlar ve fiyat araligi (filtreleme UI icin)
        var allTours = await _context.Tours
            .Where(t => t.IsActive && t.Company!.StatusId == CompanyStatuses.Ids.Approved)
            .ToListAsync();

        var destinations = allTours
            .Select(t => t.Destination)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var priceRange = new
        {
            min = allTours.Any() ? allTours.Min(t => t.Price) : 0,
            max = allTours.Any() ? allTours.Max(t => t.Price) : 0
        };

        var durationRange = new
        {
            min = allTours.Any() ? allTours.Min(t => t.DurationDays) : 1,
            max = allTours.Any() ? allTours.Max(t => t.DurationDays) : 30
        };

        return Ok(new
        {
            tours,
            filters = new
            {
                destinations,
                priceRange,
                durationRange
            },
            totalCount = tours.Count
        });
    }

    [HttpGet("featured")]
    [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language")] // 5 dakika HTTP cache
    public async Task<ActionResult<IEnumerable<Tour>>> GetFeaturedTours()
    {
        var tours = await _cache.GetOrCreateAsync(
            CacheKeys.FeaturedTours,
            async () => await _context.Tours
                .Include(t => t.Company)
                .Where(t => t.IsActive && t.IsFeatured && t.Company!.StatusId == CompanyStatuses.Ids.Approved)
                .ToListAsync(),
            CacheDurations.Medium
        );

        return Ok(tours);
    }

    /// <summary>
    /// Filtreleme icin firma listesi (sadece tur olan firmalar)
    /// </summary>
    [HttpGet("companies")]
    public async Task<ActionResult<object>> GetTourCompanies()
    {
        var companies = await _context.Companies
            .Where(c => c.IsActive && c.StatusId == CompanyStatuses.Ids.Approved)
            .Where(c => c.Tours.Any(t => t.IsActive))
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        return Ok(companies);
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

    /// <summary>
    /// Firma sahibinin kendi turlarini listele
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyTours()
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

        var tours = await _context.Tours
            .Where(t => t.CompanyId == visitor.Company.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Destination,
                t.Price,
                t.DurationDays,
                t.MaxCapacity,
                t.ImageUrl,
                t.IsFeatured,
                t.IsActive,
                t.ReviewCount,
                t.AverageRating,
                t.CreatedAt,
                ReservationCount = t.Reservations.Count(r => r.IsActive)
            })
            .ToListAsync();

        return Ok(new
        {
            tours,
            companyStatus = visitor.Company.StatusId,
            canManageTours = visitor.Company.StatusId == CompanyStatuses.Ids.Approved
        });
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
