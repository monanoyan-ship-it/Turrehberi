using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.TourWatches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/watches")]
[Authorize]
public class TourWatchesController : ControllerBase
{
    private readonly ITourWatchFactory _watchFactory;

    public TourWatchesController(ITourWatchFactory watchFactory)
    {
        _watchFactory = watchFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetWatches()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        return Ok(await _watchFactory.GetWatchedToursAsync(visitorId.Value));
    }

    [HttpGet("check/{tourId}")]
    public async Task<ActionResult<object>> CheckWatch(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Ok(new { isWatching = false });

        var isWatching = await _watchFactory.CheckWatchAsync(visitorId.Value, tourId);
        return Ok(new { isWatching });
    }

    [HttpPost("check-multiple")]
    public async Task<ActionResult<object>> CheckMultipleWatches([FromBody] int[] tourIds)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Ok(new { watchedIds = Array.Empty<int>() });

        var watchedIds = await _watchFactory.CheckMultipleWatchesAsync(visitorId.Value, tourIds);
        return Ok(new { watchedIds });
    }

    [HttpPost("{tourId}")]
    public async Task<IActionResult> AddWatch(int tourId, [FromBody] WatchRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, message) = await _watchFactory.AddWatchAsync(visitorId.Value, tourId, request.WatchDays);
        if (!success)
        {
            if (message == "Tur bulunamadi") return NotFound(new { message });
            return BadRequest(new { message });
        }
        return Ok(new { message });
    }

    [HttpDelete("{tourId}")]
    public async Task<IActionResult> RemoveWatch(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, message) = await _watchFactory.RemoveWatchAsync(visitorId.Value, tourId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    [HttpPost("{tourId}/toggle")]
    public async Task<ActionResult<object>> ToggleWatch(int tourId, [FromBody] WatchRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (isWatching, message, tourNotFound) = await _watchFactory.ToggleWatchAsync(visitorId.Value, tourId, request.WatchDays);
        if (tourNotFound) return NotFound(new { message });
        return Ok(new { isWatching, message });
    }
}

public class WatchRequest
{
    public int WatchDays { get; set; } = 7;
}
