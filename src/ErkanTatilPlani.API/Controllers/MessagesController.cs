using System.Security.Claims;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageFactory _messageFactory;

    public MessagesController(IMessageFactory messageFactory)
    {
        _messageFactory = messageFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    private int? GetCompanyId()
    {
        var claim = User.FindFirst("CompanyId")?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    private bool IsCompanyOwner()
    {
        var userType = User.FindFirst(ClaimTypes.Role)?.Value;
        return userType == "CompanyOwner";
    }

    // GET /api/messages/conversations?page=1&pageSize=20
    [HttpGet("conversations")]
    public async Task<ActionResult<object>> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        if (IsCompanyOwner())
        {
            var companyId = GetCompanyId();
            if (companyId == null) return BadRequest(new { message = "Firma bilgisi bulunamadi" });
            return Ok(await _messageFactory.GetConversationsForCompanyAsync(companyId.Value, page, pageSize));
        }

        return Ok(await _messageFactory.GetConversationsForVisitorAsync(visitorId.Value, page, pageSize));
    }

    // GET /api/messages/conversations/visitor?page=1&pageSize=20
    [HttpGet("conversations/visitor")]
    public async Task<ActionResult<object>> GetVisitorConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });
        return Ok(await _messageFactory.GetConversationsForVisitorAsync(visitorId.Value, page, pageSize));
    }

    // GET /api/messages/conversations/company?page=1&pageSize=20
    [HttpGet("conversations/company")]
    public async Task<ActionResult<object>> GetCompanyConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });
        var companyId = GetCompanyId();
        if (companyId == null) return BadRequest(new { message = "Firma bilgisi bulunamadi" });
        return Ok(await _messageFactory.GetConversationsForCompanyAsync(companyId.Value, page, pageSize));
    }

    // GET /api/messages/{conversationId}?page=1&pageSize=50
    [HttpGet("{conversationId}")]
    public async Task<ActionResult<object>> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var isCompany = IsCompanyOwner();
        return Ok(await _messageFactory.GetMessagesAsync(conversationId, visitorId.Value, isCompany, page, pageSize));
    }

    // POST /api/messages/conversations
    [HttpPost("conversations")]
    public async Task<ActionResult<object>> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, message, data) = await _messageFactory.CreateConversationAsync(
            visitorId.Value, request.CompanyId, request.TourId, request.Subject, request.Message);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // POST /api/messages
    [HttpPost]
    public async Task<ActionResult<object>> SendMessage([FromBody] SendMessageRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var senderTypeId = IsCompanyOwner() ? MessageSenderTypes.Ids.Company : MessageSenderTypes.Ids.Visitor;
        var (success, message, data) = await _messageFactory.SendMessageAsync(
            request.ConversationId, visitorId.Value, senderTypeId, request.Content);

        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // PUT /api/messages/{conversationId}/read
    [HttpPut("{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(int conversationId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var isCompany = IsCompanyOwner();
        var (success, message) = await _messageFactory.MarkAsReadAsync(conversationId, visitorId.Value, isCompany);

        if (!success) return NotFound(new { message });
        return Ok(new { message });
    }

    // GET /api/messages/unread-count
    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Ok(new { visitorCount = 0, companyCount = 0 });

        var visitorCount = await _messageFactory.GetUnreadCountForVisitorAsync(visitorId.Value);
        var companyCount = 0;

        if (IsCompanyOwner())
        {
            var companyId = GetCompanyId();
            if (companyId != null)
                companyCount = await _messageFactory.GetUnreadCountForCompanyAsync(companyId.Value);
        }

        return Ok(new { visitorCount, companyCount });
    }
}

public class CreateConversationRequest
{
    public int CompanyId { get; set; }
    public int? TourId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendMessageRequest
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
}
