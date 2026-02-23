using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PushSubscriptionsController : ControllerBase
{
    private readonly IPushSubscriptionEntityService _subscriptionService;
    private readonly IUnitOfWork _unitOfWork;

    public PushSubscriptionsController(
        IPushSubscriptionEntityService subscriptionService,
        IUnitOfWork unitOfWork)
    {
        _subscriptionService = subscriptionService;
        _unitOfWork = unitOfWork;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscribeRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Error.LoginRequired" });

        // Ayni endpoint varsa guncelle
        var existing = await _subscriptionService.GetByEndpointAsync(request.Endpoint);
        if (existing != null)
        {
            existing.P256dh = request.P256dh;
            existing.Auth = request.Auth;
            existing.VisitorId = visitorId.Value;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.IsActive = true;
        }
        else
        {
            _subscriptionService.Add(new PushSubscription
            {
                VisitorId = visitorId.Value,
                Endpoint = request.Endpoint,
                P256dh = request.P256dh,
                Auth = request.Auth,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return Ok(new { message = "Success.PushSubscriptionSaved" });
    }

    [HttpDelete("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] PushUnsubscribeRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Error.LoginRequired" });

        var existing = await _subscriptionService.GetByEndpointAsync(request.Endpoint);
        if (existing != null && existing.VisitorId == visitorId.Value)
        {
            _subscriptionService.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
        }

        return Ok(new { message = "Success.PushSubscriptionRemoved" });
    }
}

public class PushSubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class PushUnsubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
}
