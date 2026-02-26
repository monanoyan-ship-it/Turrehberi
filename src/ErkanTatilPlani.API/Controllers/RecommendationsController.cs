using ErkanTatilPlani.Core.Factories.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly ITourRecommendationFactory _recommendationFactory;

    public RecommendationsController(ITourRecommendationFactory recommendationFactory)
    {
        _recommendationFactory = recommendationFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<object>> GetRecommendations([FromQuery] int limit = 5)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _recommendationFactory.GetRecommendationsAsync(visitorId.Value, limit);
        return Ok(result);
    }
}
