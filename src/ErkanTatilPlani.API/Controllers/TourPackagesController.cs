using ErkanTatilPlani.Core.Factories.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

/// <summary>
/// Tur paketi olusturucu (Faz 15.6)
/// </summary>
[ApiController]
[Route("api/packages")]
[Tags("Tour Packages")]
public class TourPackagesController : ControllerBase
{
    private readonly ITourPackageFactory _packageFactory;

    public TourPackagesController(ITourPackageFactory packageFactory)
    {
        _packageFactory = packageFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    /// <summary>
    /// Herkese acik paketler
    /// </summary>
    [HttpGet("public")]
    public async Task<ActionResult<object>> GetPublicPackages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (_, result, _) = await _packageFactory.GetPublicPackagesAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Kendi paketlerim
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyPackages()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (_, result, _) = await _packageFactory.GetMyPackagesAsync(visitorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Paket detay
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetPackage(int id)
    {
        var (success, result, statusCode) = await _packageFactory.GetPackageByIdAsync(id);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Yeni paket olustur
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<object>> CreatePackage([FromBody] CreatePackageRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _packageFactory.CreatePackageAsync(
            visitorId.Value, request.Name, request.Description, request.IsPublic, request.Items);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Paketi sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _packageFactory.DeletePackageAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }
}

public class CreatePackageRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public List<PackageItemInput> Items { get; set; } = new();
}
