using ErkanTatilPlani.Core.Factories.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

/// <summary>
/// Tur sonrasi dijital icerik (Faz 15.7)
/// </summary>
[ApiController]
[Route("api/tours/{tourId}/content")]
[Tags("Digital Content")]
public class TourDigitalContentController : ControllerBase
{
    private readonly ITourDigitalContentFactory _contentFactory;

    public TourDigitalContentController(ITourDigitalContentFactory contentFactory)
    {
        _contentFactory = contentFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    /// <summary>
    /// Tur dijital iceriklerini listele
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetContents(int tourId)
    {
        var visitorId = GetVisitorId();
        var (success, result, statusCode) = await _contentFactory.GetTourContentsAsync(tourId, visitorId);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Dijital icerik ekle/guncelle (firma sahibi)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "CompanyOwner")]
    public async Task<ActionResult<object>> ManageContent(int tourId, [FromBody] ManageContentRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _contentFactory.ManageContentAsync(
            visitorId.Value, tourId, request.ContentId, request.Title, request.Description,
            request.ContentTypeId, request.FileUrl, request.Content, request.SortOrder, request.RequiresReservation);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Dijital icerik sil (firma sahibi)
    /// </summary>
    [HttpDelete("{contentId}")]
    [Authorize(Roles = "CompanyOwner")]
    public async Task<IActionResult> DeleteContent(int tourId, int contentId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _contentFactory.DeleteContentAsync(visitorId.Value, contentId);
        return StatusCode(statusCode, result);
    }
}

public class ManageContentRequest
{
    public int? ContentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ContentTypeId { get; set; }
    public string? FileUrl { get; set; }
    public string? Content { get; set; }
    public int SortOrder { get; set; }
    public bool RequiresReservation { get; set; } = true;
}
