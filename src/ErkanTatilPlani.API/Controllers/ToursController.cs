using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly ITourFactory _tourFactory;
    private readonly ITourPhotoFactory _photoFactory;

    public ToursController(ITourFactory tourFactory, ITourPhotoFactory photoFactory)
    {
        _tourFactory = tourFactory;
        _photoFactory = photoFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetTours(
        [FromQuery] string? search = null, [FromQuery] string? destination = null,
        [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minDays = null, [FromQuery] int? maxDays = null,
        [FromQuery] int? companyId = null, [FromQuery] bool? featured = null,
        [FromQuery] string? sort = null,
        [FromQuery] int? difficulty = null, [FromQuery] int? category = null,
        [FromQuery] string? guideLanguage = null)
    {
        return Ok(await _tourFactory.GetToursAsync(search, destination, minPrice, maxPrice, minDays, maxDays, companyId, featured, sort, difficulty, category, guideLanguage));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<object>>> GetTourCategories()
    {
        return Ok(await _tourFactory.GetTourCategoriesAsync());
    }

    [HttpGet("featured")]
    [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language")]
    public async Task<ActionResult<IEnumerable<Tour>>> GetFeaturedTours()
    {
        return Ok(await _tourFactory.GetFeaturedToursAsync());
    }

    [HttpGet("companies")]
    public async Task<ActionResult<object>> GetTourCompanies()
    {
        return Ok(await _tourFactory.GetTourCompaniesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tour>> GetTour(int id)
    {
        var tour = await _tourFactory.GetTourAsync(id);
        if (tour == null) return NotFound();
        return tour;
    }

    [HttpGet("my")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<object>> GetMyTours()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (result, errorMessage, errorCode, statusCode) = await _tourFactory.GetMyToursAsync(visitorId.Value);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<Tour>> CreateTour(Tour tour)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (created, errorMessage, errorCode, statusCode) = await _tourFactory.CreateTourAsync(visitorId.Value, tour);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return CreatedAtAction(nameof(GetTour), new { id = created!.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdateTour(int id, Tour tour)
    {
        if (id != tour.Id) return BadRequest();
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, errorMessage, errorCode, statusCode) = await _tourFactory.UpdateTourAsync(visitorId.Value, id, tour);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeleteTour(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, notFound, errorMessage, errorCode, statusCode) = await _tourFactory.DeleteTourAsync(visitorId.Value, id);
        if (notFound) return NotFound();
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    // ===============================================
    // TUR FOTOGRAFLARI
    // ===============================================

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<object>> GetTourPhotos(int id)
    {
        var (found, result) = await _photoFactory.GetPhotosAsync(id);
        if (!found)
            return NotFound(new { message = "Error.TourNotFound" });
        return Ok(result);
    }

    [HttpPost("{id}/photos")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<object>> UploadTourPhoto(int id, [FromForm] IFormFile file, [FromForm] string? title = null, [FromForm] bool isCover = false)
    {
        using var stream = file?.OpenReadStream() ?? Stream.Null;
        var (success, result, statusCode) = await _photoFactory.UploadPhotoAsync(
            id, stream, file?.FileName ?? string.Empty, title, isCover);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("{tourId}/photos/{photoId}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeleteTourPhoto(int tourId, int photoId)
    {
        var (success, result, statusCode) = await _photoFactory.DeletePhotoAsync(tourId, photoId);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{tourId}/photos/{photoId}/cover")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> SetCoverPhoto(int tourId, int photoId)
    {
        var (success, result, statusCode) = await _photoFactory.SetCoverPhotoAsync(tourId, photoId);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}/photos/reorder")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> ReorderTourPhotos(int id, [FromBody] ReorderPhotosRequest request)
    {
        var (success, result, statusCode) = await _photoFactory.ReorderPhotosAsync(id, request.PhotoIds);
        return StatusCode(statusCode, result);
    }

    [HttpPost("{id}/cover-photo")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UploadCoverPhoto(int id, [FromForm] IFormFile file)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        using var stream = file?.OpenReadStream() ?? Stream.Null;
        var (success, result, statusCode) = await _tourFactory.UploadCoverPhotoAsync(
            visitorId.Value, id, stream, file?.FileName ?? string.Empty);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("{id}/cover-photo")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeleteCoverPhoto(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _tourFactory.DeleteCoverPhotoAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }
}

public class ReorderPhotosRequest
{
    public List<int> PhotoIds { get; set; } = new();
}
