using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/companies")]
public class CompanyPagesController : ControllerBase
{
    private readonly ICompanyPageFactory _pageFactory;

    public CompanyPagesController(ICompanyPageFactory pageFactory)
    {
        _pageFactory = pageFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    // ============================================
    // FIRMA SAHIBI ENDPOINTS
    // ============================================

    [HttpGet("{companyId:int}/pages")]
    [Authorize]
    public async Task<IActionResult> GetPages(int companyId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (result, errorMessage, errorCode, statusCode) = await _pageFactory.GetPagesAsync(visitorId.Value, companyId);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    [HttpGet("{companyId:int}/pages/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetPage(int companyId, int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (result, errorMessage, errorCode, statusCode) = await _pageFactory.GetPageAsync(visitorId.Value, companyId, id);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    [HttpPost("{companyId:int}/pages")]
    [Authorize]
    public async Task<IActionResult> CreatePage(int companyId, [FromBody] CompanyPage page)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (created, errorMessage, errorCode, statusCode) = await _pageFactory.CreatePageAsync(visitorId.Value, companyId, page);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(created);
    }

    [HttpPut("{companyId:int}/pages/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePage(int companyId, int id, [FromBody] CompanyPage page)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _pageFactory.UpdatePageAsync(visitorId.Value, companyId, id, page);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpDelete("{companyId:int}/pages/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePage(int companyId, int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _pageFactory.DeletePageAsync(visitorId.Value, companyId, id);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpPut("{companyId:int}/pages/reorder")]
    [Authorize]
    public async Task<IActionResult> ReorderPages(int companyId, [FromBody] List<int> pageIds)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _pageFactory.ReorderPagesAsync(visitorId.Value, companyId, pageIds);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    // ============================================
    // PUBLIC ENDPOINTS
    // ============================================

    [HttpGet("pages/{companySlug}")]
    public async Task<IActionResult> GetPublicPages(string companySlug)
    {
        var result = await _pageFactory.GetPublicPagesAsync(companySlug);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("pages/{companySlug}/{pageSlug}")]
    public async Task<IActionResult> GetPublicPage(string companySlug, string pageSlug)
    {
        var result = await _pageFactory.GetPublicPageBySlugAsync(companySlug, pageSlug);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
