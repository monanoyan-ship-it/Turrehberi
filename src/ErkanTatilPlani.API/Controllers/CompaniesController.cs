using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyCrudFactory _crud;
    private readonly ICompanyProfileFactory _profile;
    private readonly ICompanyApprovalFactory _approval;
    private readonly ICompanyDashboardFactory _dashboard;
    private readonly ICompanyGalleryFactory _gallery;
    private readonly ICompanyAnalyticsFactory _analytics;

    public CompaniesController(
        ICompanyCrudFactory crud,
        ICompanyProfileFactory profile,
        ICompanyApprovalFactory approval,
        ICompanyDashboardFactory dashboard,
        ICompanyGalleryFactory gallery,
        ICompanyAnalyticsFactory analytics)
    {
        _crud = crud;
        _profile = profile;
        _approval = approval;
        _dashboard = dashboard;
        _gallery = gallery;
        _analytics = analytics;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        return Ok(await _crud.GetCompaniesAsync());
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _crud.GetCompanyAsync(id);
        if (company == null) return NotFound();
        return company;
    }

    [HttpGet("{id}/tours")]
    public async Task<ActionResult<IEnumerable<Tour>>> GetCompanyTours(int id)
    {
        return Ok(await _profile.GetCompanyToursAsync(id));
    }

    // ===============================================
    // PUBLIC FIRMA PROFILI (SEO-FRIENDLY)
    // ===============================================

    /// <summary>
    /// Public firma profili - slug ile erisim (SEO icin)
    /// </summary>
    [HttpGet("profile/{slug}")]
    public async Task<ActionResult<object>> GetCompanyProfile(string slug)
    {
        var (found, result) = await _profile.GetCompanyProfileAsync(slug);
        if (!found)
            return NotFound(new { message = "Error.CompanyNotFound" });
        return Ok(result);
    }

    /// <summary>
    /// Public firma listesi (sadece onaylanmis firmalar)
    /// </summary>
    [HttpGet("public")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "city", "search", "sort" })]
    public async Task<ActionResult<object>> GetPublicCompanies(
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null)
    {
        return Ok(await _profile.GetPublicCompaniesAsync(city, search, sort));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Company>> CreateCompany(Company company)
    {
        var created = await _crud.CreateCompanyAsync(company);
        return CreatedAtAction(nameof(GetCompany), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateCompany(int id, Company company)
    {
        if (id != company.Id) return BadRequest();
        await _crud.UpdateCompanyAsync(company);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var found = await _crud.DeleteCompanyAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }

    // ===============================================
    // FIRMA ONAY IS AKISI
    // ===============================================

    /// <summary>
    /// Onay bekleyen firmalari listele
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<Company>>> GetPendingCompanies()
    {
        return Ok(await _approval.GetPendingCompaniesAsync());
    }

    /// <summary>
    /// Firma onayla
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ApproveCompany(int id, [FromBody] ApproveCompanyRequest request)
    {
        var (success, result, statusCode) = await _approval.ApproveCompanyAsync(id, request.ReviewedById, request.ReviewNotes);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Firma reddet
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> RejectCompany(int id, [FromBody] RejectCompanyRequest request)
    {
        var (success, result, statusCode) = await _approval.RejectCompanyAsync(id, request.ReviewedById, request.RejectionReason, request.ReviewNotes);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Firma askiya al
    /// </summary>
    [HttpPost("{id}/suspend")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> SuspendCompany(int id, [FromBody] SuspendCompanyRequest request)
    {
        var (success, result, statusCode) = await _approval.SuspendCompanyAsync(id, request.ReviewedById, request.Reason);
        return StatusCode(statusCode, result);
    }

    // ===============================================
    // FIRMA DASHBOARD
    // ===============================================

    /// <summary>
    /// Firma sahibi icin dashboard istatistikleri
    /// </summary>
    [HttpGet("{id}/dashboard")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<object>> GetCompanyDashboard(int id)
    {
        var (found, result) = await _dashboard.GetCompanyDashboardAsync(id);
        if (!found)
            return NotFound(new { message = "Error.CompanyNotFound" });
        return Ok(result);
    }

    /// <summary>
    /// Firma ayarlarini guncelle
    /// </summary>
    [HttpPut("{id}/settings")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdateCompanySettings(int id, [FromBody] UpdateCompanySettingsRequest request)
    {
        var (success, result, statusCode) = await _dashboard.UpdateCompanySettingsAsync(id, request.DepositPercentage);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Askiya alinan firmayi tekrar aktifle
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ReactivateCompany(int id, [FromBody] ReactivateCompanyRequest request)
    {
        var (success, result, statusCode) = await _approval.ReactivateCompanyAsync(id, request.ReviewedById, request.ReviewNotes);
        return StatusCode(statusCode, result);
    }

    // ===============================================
    // SOZLESME YUKLEME ISLEMLERI
    // ===============================================

    /// <summary>
    /// Firma sozlesmesi yukle (PDF, max 10MB)
    /// </summary>
    [HttpPost("{id}/upload-contract")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UploadContract(int id, IFormFile file)
    {
        using var stream = file?.OpenReadStream() ?? Stream.Null;
        var (success, result, statusCode) = await _dashboard.UploadContractAsync(
            id, stream, file?.FileName ?? string.Empty, file?.ContentType ?? string.Empty, file?.Length ?? 0);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Firma sozlesmesini sil
    /// </summary>
    [HttpDelete("{id}/contract")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var (success, result, statusCode) = await _dashboard.DeleteContractAsync(id);
        return StatusCode(statusCode, result);
    }

    // ===============================================
    // FIRMA GALERI ISLEMLERI
    // ===============================================

    /// <summary>
    /// Firma galerisine resim yukle
    /// </summary>
    [HttpPost("{id}/gallery")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<object>> UploadGalleryImage(int id, [FromForm] IFormFile file, [FromForm] string? title = null, [FromForm] string? description = null, [FromForm] bool isFeatured = false)
    {
        using var stream = file?.OpenReadStream() ?? Stream.Null;
        var (success, result, statusCode) = await _gallery.UploadGalleryImageAsync(
            id, stream, file?.FileName ?? string.Empty, title, description, isFeatured);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Firma galeri resimlerini listele
    /// </summary>
    [HttpGet("{id}/gallery")]
    public async Task<ActionResult<object>> GetGalleryImages(int id)
    {
        var (found, result) = await _gallery.GetGalleryImagesAsync(id);
        if (!found)
            return NotFound(new { message = "Error.CompanyNotFound" });
        return Ok(result);
    }

    /// <summary>
    /// Galeri resmini guncelle
    /// </summary>
    [HttpPut("{companyId}/gallery/{imageId}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdateGalleryImage(int companyId, int imageId, [FromBody] UpdateGalleryImageRequest request)
    {
        var (success, result, statusCode) = await _gallery.UpdateGalleryImageAsync(
            companyId, imageId, request.Title, request.Description, request.AltText, request.DisplayOrder, request.IsFeatured);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Galeri resmini sil
    /// </summary>
    [HttpDelete("{companyId}/gallery/{imageId}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeleteGalleryImage(int companyId, int imageId)
    {
        var (success, result, statusCode) = await _gallery.DeleteGalleryImageAsync(companyId, imageId);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Galeri resim siralamasini guncelle
    /// </summary>
    [HttpPut("{id}/gallery/reorder")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> ReorderGalleryImages(int id, [FromBody] ReorderGalleryRequest request)
    {
        var (success, result, statusCode) = await _gallery.ReorderGalleryImagesAsync(id, request.ImageIds);
        return StatusCode(statusCode, result);
    }
    // ===============================================
    // ANALITIK ISLEMLERI
    // ===============================================

    [HttpGet("analytics/revenue")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetRevenueChart([FromQuery] int months = 12)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();
        var (success, result, statusCode) = await _analytics.GetRevenueChartAsync(visitorId.Value, months);
        return StatusCode(statusCode, result);
    }

    [HttpGet("analytics/occupancy")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetOccupancyChart([FromQuery] int months = 12)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();
        var (success, result, statusCode) = await _analytics.GetOccupancyChartAsync(visitorId.Value, months);
        return StatusCode(statusCode, result);
    }

    [HttpGet("analytics/cancellations")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetCancellationChart([FromQuery] int months = 12)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();
        var (success, result, statusCode) = await _analytics.GetCancellationChartAsync(visitorId.Value, months);
        return StatusCode(statusCode, result);
    }

    [HttpGet("analytics/tour-comparison")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetTourComparison()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();
        var (success, result, statusCode) = await _analytics.GetTourComparisonAsync(visitorId.Value);
        return StatusCode(statusCode, result);
    }
}

// Request DTO'lari
public class ApproveCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string? ReviewNotes { get; set; }
}

public class RejectCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
}

public class SuspendCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReactivateCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string? ReviewNotes { get; set; }
}

public class UpdateGalleryImageRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AltText { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsFeatured { get; set; }
}

public class ReorderGalleryRequest
{
    public List<int> ImageIds { get; set; } = new();
}

public class UpdateCompanySettingsRequest
{
    public int DepositPercentage { get; set; }
}
