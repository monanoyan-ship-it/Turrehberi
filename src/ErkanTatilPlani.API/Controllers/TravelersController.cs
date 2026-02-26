using ErkanTatilPlani.Core.Factories.Travelers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelersController : ControllerBase
{
    private readonly ITravelerProfileFactory _profileFactory;

    public TravelersController(ITravelerProfileFactory profileFactory)
    {
        _profileFactory = profileFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProfile(int id)
    {
        var currentVisitorId = GetVisitorId();
        var result = await _profileFactory.GetPublicProfileAsync(id, currentVisitorId);
        return Ok(result);
    }

    [HttpGet("{id}/followers")]
    public async Task<ActionResult<object>> GetFollowers(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _profileFactory.GetFollowersAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}/following")]
    public async Task<ActionResult<object>> GetFollowing(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _profileFactory.GetFollowingAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpPost("{id}/follow")]
    [Authorize]
    public async Task<IActionResult> Follow(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _profileFactory.FollowAsync(visitorId.Value, id);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpDelete("{id}/follow")]
    [Authorize]
    public async Task<IActionResult> Unfollow(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _profileFactory.UnfollowAsync(visitorId.Value, id);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpPut("bio")]
    [Authorize]
    public async Task<IActionResult> UpdateBio([FromBody] UpdateBioRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _profileFactory.UpdateBioAsync(visitorId.Value, request.Bio ?? "");
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpPut("visibility")]
    [Authorize]
    public async Task<IActionResult> ToggleVisibility()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _profileFactory.ToggleProfileVisibilityAsync(visitorId.Value);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpGet("top")]
    public async Task<ActionResult<object>> GetTopTravelers([FromQuery] int limit = 10)
    {
        var result = await _profileFactory.GetTopTravelersAsync(limit);
        return Ok(result);
    }
}

public class UpdateBioRequest
{
    public string? Bio { get; set; }
}
