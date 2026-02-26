using ErkanTatilPlani.Core.Factories.AbandonedCarts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AbandonedCartsController : ControllerBase
{
    private readonly IAbandonedCartFactory _cartFactory;

    public AbandonedCartsController(IAbandonedCartFactory cartFactory)
    {
        _cartFactory = cartFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpPost("track")]
    public async Task<ActionResult> TrackCart([FromBody] TrackCartRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Validation.EmailRequired" });

        var visitorId = GetVisitorId();
        await _cartFactory.TrackCartAsync(visitorId, request.Email, request.TourId, request.ScheduleId, request.NumberOfPeople, request.Price, request.DateToken);
        return Ok(new { success = true });
    }
}

public class TrackCartRequest
{
    public string Email { get; set; } = string.Empty;
    public int TourId { get; set; }
    public int? ScheduleId { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal Price { get; set; }
    public string? DateToken { get; set; }
}
