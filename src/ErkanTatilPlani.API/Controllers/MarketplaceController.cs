using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceFinanceFactory _marketplace;

    public MarketplaceController(IMarketplaceFinanceFactory marketplace)
    {
        _marketplace = marketplace;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet("admin/overview")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminOverview()
        => Ok(await _marketplace.GetAdminOverviewAsync());

    [HttpGet("admin/sellers")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminSellers()
        => Ok(await _marketplace.GetAdminSellersAsync());

    [HttpGet("admin/transactions")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminTransactions([FromQuery] int? companyId = null, [FromQuery] int? statusId = null)
        => Ok(await _marketplace.GetAdminTransactionsAsync(companyId, statusId));

    [HttpGet("admin/refunds")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminRefunds([FromQuery] int? companyId = null, [FromQuery] int? statusId = null)
        => Ok(await _marketplace.GetAdminRefundsAsync(companyId, statusId));

    [HttpGet("admin/payouts")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminPayouts([FromQuery] int? companyId = null, [FromQuery] int? statusId = null)
        => Ok(await _marketplace.GetAdminPayoutsAsync(companyId, statusId));

    [HttpPut("admin/sellers/{companyId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateSellerSettings(int companyId, [FromBody] MarketplaceSellerSettingsRequest request)
    {
        var (success, result, statusCode) = await _marketplace.UpdateSellerSettingsAsync(companyId, request);
        return StatusCode(statusCode, result);
    }

    [HttpPost("admin/sellers/{companyId}/onboard")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> OnboardSeller(int companyId)
    {
        var (success, result, statusCode) = await _marketplace.OnboardSellerAsync(companyId);
        return StatusCode(statusCode, result);
    }

    [HttpPost("admin/transactions/{transactionId}/refund")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateRefund(int transactionId, [FromBody] CreateMarketplaceRefundRequest request)
    {
        var (success, result, statusCode) = await _marketplace.CreateRefundAsync(transactionId, request, GetVisitorId());
        return StatusCode(statusCode, result);
    }

    [HttpPost("admin/sellers/{companyId}/payouts")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreatePayout(int companyId, [FromBody] CreatePayoutBatchRequest request)
    {
        var (success, result, statusCode) = await _marketplace.CreatePayoutBatchAsync(companyId, request, GetVisitorId());
        return StatusCode(statusCode, result);
    }

    [HttpPost("admin/payouts/{payoutId}/mark-paid")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> MarkPayoutPaid(int payoutId, [FromBody] MarkPayoutPaidRequest request)
    {
        var (success, result, statusCode) = await _marketplace.MarkPayoutPaidAsync(payoutId, request, GetVisitorId());
        return StatusCode(statusCode, result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> GetMyFinance()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _marketplace.GetCompanyOverviewAsync(visitorId.Value);
        return StatusCode(statusCode, result);
    }

    [HttpPut("my/settings")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdateMySellerSettings([FromBody] MarketplaceSellerSettingsRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _marketplace.UpdateMySellerSettingsAsync(visitorId.Value, request);
        return StatusCode(statusCode, result);
    }

    [HttpPost("my/onboard")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> OnboardMySeller()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _marketplace.OnboardMySellerAsync(visitorId.Value);
        return StatusCode(statusCode, result);
    }
}
