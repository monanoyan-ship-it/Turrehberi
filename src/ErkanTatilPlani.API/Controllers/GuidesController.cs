using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Guides;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "CompanyOwner,Staff,Admin")]
public class GuidesController : ControllerBase
{
    private readonly IGuideFactory _guideFactory;

    public GuidesController(IGuideFactory guideFactory)
    {
        _guideFactory = guideFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanyGuides()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.GetCompanyGuidesAsync(visitorId.Value);
        return StatusCode(statusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGuide(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.GetGuideByIdAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGuide([FromBody] Guide guide)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.CreateGuideAsync(visitorId.Value, guide);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGuide(int id, [FromBody] Guide guide)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.UpdateGuideAsync(visitorId.Value, id, guide);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGuide(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.DeleteGuideAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignToDate(int id, [FromBody] AssignGuideRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.AssignGuideToDateAsync(
            visitorId.Value, id, request.TourDateId, request.Notes);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("assignments/{id}")]
    public async Task<IActionResult> RemoveAssignment(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.RemoveAssignmentAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpGet("{id}/availability")]
    public async Task<IActionResult> GetAvailability(int id, [FromQuery] int year, [FromQuery] int month)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.GetGuideAvailabilityAsync(
            visitorId.Value, id, year, month);
        return StatusCode(statusCode, result);
    }

    [HttpGet("{id}/performance")]
    public async Task<IActionResult> GetPerformance(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _guideFactory.GetGuidePerformanceAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }
}

public class AssignGuideRequest
{
    public int TourDateId { get; set; }
    public string? Notes { get; set; }
}
