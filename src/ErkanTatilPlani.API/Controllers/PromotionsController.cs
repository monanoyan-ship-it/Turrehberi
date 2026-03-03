using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionCrudFactory _crudFactory;
    private readonly IPromotionCalculationFactory _calculationFactory;

    public PromotionsController(
        IPromotionCrudFactory crudFactory,
        IPromotionCalculationFactory calculationFactory)
    {
        _crudFactory = crudFactory;
        _calculationFactory = calculationFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    // ===== CRUD (Auth) =====

    [HttpGet]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetCompanyPromotions(
        [FromQuery] int? type = null, [FromQuery] int? status = null)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _crudFactory.GetCompanyPromotionsAsync(visitorId.Value, type, status);
        return StatusCode(statusCode, result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetPromotion(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _crudFactory.GetPromotionByIdAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpPost]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> CreatePromotion([FromBody] Promotion promotion)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _crudFactory.CreatePromotionAsync(visitorId.Value, promotion);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdatePromotion(int id, [FromBody] Promotion promotion)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _crudFactory.UpdatePromotionAsync(visitorId.Value, id, promotion);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _crudFactory.DeletePromotionAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpPut("{id}/toggle")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var (success, result, statusCode) = await _crudFactory.ToggleStatusAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    // ===== Admin =====

    [HttpGet("/api/admin/promotions")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAllPromotions(
        [FromQuery] int? companyId = null, [FromQuery] int? type = null, [FromQuery] int? status = null)
    {
        var (success, result, statusCode) = await _crudFactory.GetAllPromotionsAsync(companyId, type, status);
        return StatusCode(statusCode, result);
    }

    // ===== Public =====

    [HttpPost("/api/tours/{tourId}/calculate-price")]
    public async Task<IActionResult> CalculatePrice(int tourId, [FromBody] CalculatePriceRequest request)
    {
        var visitorId = GetVisitorId();
        var result = await _calculationFactory.CalculatePriceAsync(
            tourId, request.NumberOfPeople, request.StartDate, request.CouponCode, visitorId, request.ScheduleId);
        return Ok(result);
    }

    [HttpPost("/api/tours/{tourId}/validate-coupon")]
    public async Task<IActionResult> ValidateCoupon(int tourId, [FromBody] ValidateCouponRequest request)
    {
        var visitorId = GetVisitorId();
        var totalPrice = request.NumberOfPeople * 1; // Will be calculated inside
        var (valid, discountAmount, errorMessage) = await _calculationFactory.ValidateCouponAsync(
            request.Code, tourId, request.NumberOfPeople, totalPrice, visitorId);

        if (valid)
            return Ok(new { valid = true, discountAmount });
        else
            return Ok(new { valid = false, message = errorMessage });
    }

    [HttpGet("/api/tours/{tourId}/promotion-badges")]
    public async Task<IActionResult> GetPromotionBadges(int tourId, [FromQuery] DateTime? startDate = null)
    {
        var result = await _calculationFactory.GetTourPromotionBadgesAsync(tourId, startDate);
        return Ok(result);
    }

    [HttpGet("flash-sales")]
    public async Task<IActionResult> GetFlashSales()
    {
        var result = await _calculationFactory.GetActiveFlashSalesAsync();
        return Ok(result);
    }

    [HttpGet("last-minute")]
    public async Task<IActionResult> GetLastMinuteDeals()
    {
        var result = await _calculationFactory.GetLastMinuteDealsAsync();
        return Ok(result);
    }

    [HttpGet("types")]
    public IActionResult GetPromotionTypes()
    {
        var types = PromotionTypes.All.Select(t => new
        {
            t.Id,
            t.SystemName,
            t.NameResourceKey,
            t.Icon,
            t.CssClass
        });
        return Ok(types);
    }
}

public class CalculatePriceRequest
{
    public int NumberOfPeople { get; set; }
    public DateTime? StartDate { get; set; }
    public string? CouponCode { get; set; }
    public int? ScheduleId { get; set; }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
}
