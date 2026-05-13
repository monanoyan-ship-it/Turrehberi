using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentFactory _paymentFactory;

    public PaymentsController(IPaymentFactory paymentFactory)
    {
        _paymentFactory = paymentFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpPost("initialize/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> InitializePayment(int reservationId, [FromBody] InitializePaymentRequest? request = null)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _paymentFactory.InitializePaymentAsync(
            visitorId.Value,
            reservationId,
            Request.Scheme,
            Request.Host.ToString(),
            request?.PaymentMethodSystemName);
        return StatusCode(statusCode, result);
    }

    [HttpPost("initialize-remaining/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> InitializeRemainingPayment(int reservationId, [FromBody] InitializePaymentRequest? request = null)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _paymentFactory.InitializeRemainingPaymentAsync(
            visitorId.Value,
            reservationId,
            Request.Scheme,
            Request.Host.ToString(),
            request?.PaymentMethodSystemName);
        return StatusCode(statusCode, result);
    }

    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromForm] string token)
    {
        var (success, redirectUrl) = await _paymentFactory.ProcessCallbackAsync(token);
        return Redirect(redirectUrl!);
    }

    [HttpGet("status/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetPaymentStatus(int reservationId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var result = await _paymentFactory.GetPaymentStatusAsync(visitorId.Value, reservationId);
        if (result == null) return NotFound(new { message = "Error.ReservationNotFound" });
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize]
    public async Task<ActionResult<object>> GetPendingPayments()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        return Ok(await _paymentFactory.GetPendingPaymentsAsync(visitorId.Value));
    }
}

public class InitializePaymentRequest
{
    public string? PaymentMethodSystemName { get; set; }
}
