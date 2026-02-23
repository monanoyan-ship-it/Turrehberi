using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteFactory _favoriteFactory;

    public FavoritesController(IFavoriteFactory favoriteFactory)
    {
        _favoriteFactory = favoriteFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetFavorites()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Error.LoginRequired" });

        return Ok(await _favoriteFactory.GetFavoritesAsync(visitorId.Value));
    }

    [HttpGet("check/{tourId}")]
    public async Task<ActionResult<object>> CheckFavorite(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Ok(new { isFavorite = false });

        var isFavorite = await _favoriteFactory.CheckFavoriteAsync(visitorId.Value, tourId);
        return Ok(new { isFavorite });
    }

    [HttpPost("check-multiple")]
    public async Task<ActionResult<object>> CheckMultipleFavorites([FromBody] int[] tourIds)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Ok(new { favoriteIds = Array.Empty<int>() });

        var favoriteIds = await _favoriteFactory.CheckMultipleFavoritesAsync(visitorId.Value, tourIds);
        return Ok(new { favoriteIds });
    }

    [HttpPost("{tourId}")]
    public async Task<IActionResult> AddFavorite(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, message) = await _favoriteFactory.AddFavoriteAsync(visitorId.Value, tourId);
        if (!success)
        {
            if (message == "Error.TourNotFound") return NotFound(new { message });
            return BadRequest(new { message });
        }
        return Ok(new { message });
    }

    [HttpDelete("{tourId}")]
    public async Task<IActionResult> RemoveFavorite(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, message) = await _favoriteFactory.RemoveFavoriteAsync(visitorId.Value, tourId);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    [HttpPost("{tourId}/toggle")]
    public async Task<ActionResult<object>> ToggleFavorite(int tourId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Error.LoginRequired" });

        var (isFavorite, message, tourNotFound) = await _favoriteFactory.ToggleFavoriteAsync(visitorId.Value, tourId);
        if (tourNotFound) return NotFound(new { message });
        return Ok(new { isFavorite, message });
    }
}
