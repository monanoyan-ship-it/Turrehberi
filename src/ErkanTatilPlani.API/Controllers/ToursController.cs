using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly ITourFactory _tourFactory;

    public ToursController(ITourFactory tourFactory)
    {
        _tourFactory = tourFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetTours(
        [FromQuery] string? search = null, [FromQuery] string? destination = null,
        [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minDays = null, [FromQuery] int? maxDays = null,
        [FromQuery] int? companyId = null, [FromQuery] bool? featured = null,
        [FromQuery] string? sort = null,
        [FromQuery] int? difficulty = null, [FromQuery] int? category = null,
        [FromQuery] string? guideLanguage = null)
    {
        return Ok(await _tourFactory.GetToursAsync(search, destination, minPrice, maxPrice, minDays, maxDays, companyId, featured, sort, difficulty, category, guideLanguage));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<object>>> GetTourCategories()
    {
        return Ok(await _tourFactory.GetTourCategoriesAsync());
    }

    [HttpGet("featured")]
    [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language")]
    public async Task<ActionResult<IEnumerable<Tour>>> GetFeaturedTours()
    {
        return Ok(await _tourFactory.GetFeaturedToursAsync());
    }

    [HttpGet("companies")]
    public async Task<ActionResult<object>> GetTourCompanies()
    {
        return Ok(await _tourFactory.GetTourCompaniesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tour>> GetTour(int id)
    {
        var tour = await _tourFactory.GetTourAsync(id);
        if (tour == null) return NotFound();
        return tour;
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyTours()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (result, errorMessage, errorCode, statusCode) = await _tourFactory.GetMyToursAsync(visitorId.Value);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Tour>> CreateTour(Tour tour)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (created, errorMessage, errorCode, statusCode) = await _tourFactory.CreateTourAsync(visitorId.Value, tour);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return CreatedAtAction(nameof(GetTour), new { id = created!.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTour(int id, Tour tour)
    {
        if (id != tour.Id) return BadRequest();
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _tourFactory.UpdateTourAsync(visitorId.Value, id, tour);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTour(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, notFound, errorMessage, errorCode, statusCode) = await _tourFactory.DeleteTourAsync(visitorId.Value, id);
        if (notFound) return NotFound();
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }
}
