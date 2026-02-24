using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.TourDates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api")]
public class TourDatesController : ControllerBase
{
    private readonly ITourScheduleFactory _scheduleFactory;

    public TourDatesController(ITourScheduleFactory scheduleFactory)
    {
        _scheduleFactory = scheduleFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    // ===============================================
    // PUBLIC ENDPOINT'LER (Tur tarihleri)
    // ===============================================

    [HttpGet("tours/{tourId}/dates")]
    public async Task<ActionResult<IEnumerable<object>>> GetTourDates(int tourId)
    {
        return Ok(await _scheduleFactory.GetTourDatesAsync(tourId));
    }

    [HttpGet("tours/{tourId}/dates/cheapest")]
    public async Task<ActionResult<IEnumerable<object>>> GetCheapestDates(int tourId, [FromQuery] string month)
    {
        if (string.IsNullOrWhiteSpace(month))
            return BadRequest(new { message = "Error.MonthParameterRequired" });

        return Ok(await _scheduleFactory.GetCheapestDatesAsync(tourId, month));
    }

    // ===============================================
    // TAKVIM (Firma paneli)
    // ===============================================

    [HttpGet("tour-calendar")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetCalendarData([FromQuery] int year, [FromQuery] int month, [FromQuery] int? tourId = null)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _scheduleFactory.GetCalendarDataAsync(visitorId.Value, year, month, tourId);
        return StatusCode(statusCode, result);
    }

    // ===============================================
    // SCHEDULE (TAKVIM SABLONU) ENDPOINT'LERI
    // ===============================================

    [HttpGet("tours/{tourId}/schedules")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetSchedules(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _scheduleFactory.GetSchedulesAsync(visitorId.Value, tourId);
        return StatusCode(statusCode, result);
    }

    [HttpPost("tours/{tourId}/schedules")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> CreateSchedule(int tourId, [FromBody] CreateScheduleRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _scheduleFactory.CreateScheduleAsync(visitorId.Value, tourId, request);
        return StatusCode(statusCode, result);
    }

    [HttpPut("tour-schedules/{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _scheduleFactory.UpdateScheduleAsync(visitorId.Value, id, request);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("tour-schedules/{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _scheduleFactory.DeleteScheduleAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpPost("tours/{tourId}/dates/cancel")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> CancelDate(int tourId, [FromBody] CancelDateRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _scheduleFactory.CancelDateAsync(visitorId.Value, tourId, request);
        return StatusCode(statusCode, result);
    }
}
