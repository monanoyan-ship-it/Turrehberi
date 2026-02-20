using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.TourDates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api")]
public class TourDatesController : ControllerBase
{
    private readonly ITourDateFactory _tourDateFactory;
    private readonly ITourCalendarFactory _tourCalendarFactory;

    public TourDatesController(ITourDateFactory tourDateFactory, ITourCalendarFactory tourCalendarFactory)
    {
        _tourDateFactory = tourDateFactory;
        _tourCalendarFactory = tourCalendarFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet("tours/{tourId}/dates")]
    public async Task<ActionResult<IEnumerable<object>>> GetTourDates(int tourId)
    {
        return Ok(await _tourDateFactory.GetTourDatesAsync(tourId));
    }

    [HttpGet("tours/{tourId}/dates/manage")]
    [Authorize]
    public async Task<ActionResult<object>> ManageTourDates(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (result, errorMessage, errorCode, statusCode) = await _tourDateFactory.ManageTourDatesAsync(visitorId.Value, tourId);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    [HttpPost("tours/{tourId}/dates")]
    [Authorize]
    public async Task<ActionResult<TourDate>> CreateTourDate(int tourId, TourDate tourDate)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        tourDate.TourId = tourId;
        var (created, errorMessage, errorCode, statusCode) = await _tourDateFactory.CreateTourDateAsync(visitorId.Value, tourDate);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(created);
    }

    [HttpPut("tour-dates/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTourDate(int id, TourDate tourDate)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _tourDateFactory.UpdateTourDateAsync(visitorId.Value, id, tourDate);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpDelete("tour-dates/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTourDate(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, notFound, errorMessage, errorCode, statusCode) = await _tourDateFactory.DeleteTourDateAsync(visitorId.Value, id);
        if (notFound) return NotFound();
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpGet("tour-calendar")]
    [Authorize]
    public async Task<IActionResult> GetCalendarData([FromQuery] int year, [FromQuery] int month, [FromQuery] int? tourId = null)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, result, statusCode) = await _tourCalendarFactory.GetCalendarDataAsync(visitorId.Value, year, month, tourId);
        return StatusCode(statusCode, result);
    }

    [HttpGet("tours/{tourId}/dates/capacity-summary")]
    [Authorize]
    public async Task<IActionResult> GetCapacitySummary(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, result, statusCode) = await _tourDateFactory.GetCapacitySummaryAsync(visitorId.Value, tourId);
        return StatusCode(statusCode, result);
    }

    [HttpGet("tours/{tourId}/dates/cheapest")]
    public async Task<ActionResult<IEnumerable<object>>> GetCheapestDates(int tourId, [FromQuery] string month)
    {
        if (string.IsNullOrWhiteSpace(month))
            return BadRequest(new { message = "month parametresi gerekli (ornek: 2026-03)" });

        return Ok(await _tourDateFactory.GetCheapestDatesAsync(tourId, month));
    }
}
