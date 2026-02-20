using System.Security.Claims;
using ErkanTatilPlani.Core.Factories.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationFactory _notificationFactory;

    public NotificationsController(INotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        return Ok(await _notificationFactory.GetNotificationsAsync(visitorId.Value, page, pageSize));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Ok(new { count = 0 });

        var count = await _notificationFactory.GetUnreadCountAsync(visitorId.Value);
        return Ok(new { count });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, message) = await _notificationFactory.MarkAsReadAsync(visitorId.Value, id);
        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null)
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, message) = await _notificationFactory.MarkAllAsReadAsync(visitorId.Value);
        return Ok(new { message });
    }
}
