using ErkanTatilPlani.Core.Factories.Loyalty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyFactory _loyaltyFactory;

    public LoyaltyController(ILoyaltyFactory loyaltyFactory)
    {
        _loyaltyFactory = loyaltyFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet("dashboard")]
    [Authorize]
    public async Task<ActionResult<object>> GetDashboard()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _loyaltyFactory.GetLoyaltyDashboardAsync(visitorId.Value);
        return Ok(result);
    }

    [HttpGet("credits")]
    [Authorize]
    public async Task<ActionResult<object>> GetCreditHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _loyaltyFactory.GetCreditHistoryAsync(visitorId.Value, page, pageSize);
        return Ok(result);
    }
}
