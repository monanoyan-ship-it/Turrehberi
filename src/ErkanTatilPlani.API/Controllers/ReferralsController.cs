using ErkanTatilPlani.Core.Factories.Referrals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferralsController : ControllerBase
{
    private readonly IReferralFactory _referralFactory;

    public ReferralsController(IReferralFactory referralFactory)
    {
        _referralFactory = referralFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet("my-code")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyReferralCode()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var code = await _referralFactory.GetOrCreateReferralCodeAsync(visitorId.Value);
        return Ok(new { referralCode = code });
    }

    [HttpGet("dashboard")]
    [Authorize]
    public async Task<ActionResult<object>> GetDashboard()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _referralFactory.GetReferralDashboardAsync(visitorId.Value);
        return Ok(result);
    }

    [HttpPost("apply")]
    public async Task<ActionResult> ApplyReferralCode([FromBody] ApplyReferralRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReferralCode) || request.NewVisitorId <= 0)
            return BadRequest(new { message = "Validation.InvalidReferralData" });

        await _referralFactory.ProcessReferralSignupAsync(request.NewVisitorId, request.ReferralCode);
        return Ok(new { success = true });
    }
}

public class ApplyReferralRequest
{
    public int NewVisitorId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
}
