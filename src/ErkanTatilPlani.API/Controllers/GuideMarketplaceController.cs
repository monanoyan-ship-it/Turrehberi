using ErkanTatilPlani.Core.Factories.Guides;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

/// <summary>
/// Rehber pazaryeri - herkese acik rehber katalogu (Faz 15.3)
/// </summary>
[ApiController]
[Route("api/guide-marketplace")]
[Tags("Guide Marketplace")]
public class GuideMarketplaceController : ControllerBase
{
    private readonly IGuideMarketplaceFactory _marketplace;

    public GuideMarketplaceController(IGuideMarketplaceFactory marketplace)
    {
        _marketplace = marketplace;
    }

    /// <summary>
    /// Herkese acik rehber listesi
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetGuides(
        [FromQuery] string? language = null,
        [FromQuery] string? destination = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _marketplace.GetPublicGuidesAsync(language, destination, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Rehber profili detay
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetGuideProfile(int id)
    {
        var result = await _marketplace.GetGuideProfileAsync(id);
        return Ok(result);
    }
}
