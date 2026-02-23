using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Messages;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Messages;

public class MessageFactory : IMessageFactory
{
    private readonly IMessageEntityService _messageService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ICompanyEntityService _companyService;
    private readonly IUnitOfWork _unitOfWork;

    public MessageFactory(
        IMessageEntityService messageService,
        IVisitorEntityService visitorService,
        ICompanyEntityService companyService,
        IUnitOfWork unitOfWork)
    {
        _messageService = messageService;
        _visitorService = visitorService;
        _companyService = companyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetConversationsForVisitorAsync(int visitorId, int page, int pageSize)
    {
        var query = _messageService.GetConversationsByVisitorId(visitorId)
            .Where(c => !c.IsClosedByVisitor)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

        var totalCount = await query.CountAsync();
        var conversations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Subject,
                CompanyName = c.Company.Name,
                CompanyLogo = c.Company.LogoUrl,
                c.CompanyId,
                TourName = c.Tour != null ? c.Tour.Name : null,
                c.TourId,
                c.LastMessageAt,
                c.CreatedAt,
                c.IsClosedByCompany,
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.IsActive && m.SenderTypeId == MessageSenderTypes.Ids.Company),
                LastMessage = c.Messages
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new { m.Content, m.SenderTypeId, m.CreatedAt })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new { conversations, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<object> GetConversationsForCompanyAsync(int companyId, int page, int pageSize)
    {
        var query = _messageService.GetConversationsByCompanyId(companyId)
            .Where(c => !c.IsClosedByCompany)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

        var totalCount = await query.CountAsync();
        var conversations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Subject,
                VisitorName = c.Visitor.FirstName + " " + c.Visitor.LastName,
                VisitorAvatar = c.Visitor.AvatarUrl,
                c.VisitorId,
                TourName = c.Tour != null ? c.Tour.Name : null,
                c.TourId,
                c.LastMessageAt,
                c.CreatedAt,
                c.IsClosedByVisitor,
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.IsActive && m.SenderTypeId == MessageSenderTypes.Ids.Visitor),
                LastMessage = c.Messages
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new { m.Content, m.SenderTypeId, m.CreatedAt })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new { conversations, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<object> GetMessagesAsync(int conversationId, int requesterId, bool isCompany, int page, int pageSize)
    {
        var conversation = await _messageService.GetConversationByIdAsync(conversationId);
        if (conversation == null)
            return new { messages = Array.Empty<object>(), totalCount = 0 };

        // Yetki kontrolu
        if (isCompany)
        {
            var visitor = await _visitorService.GetByIdAsync(requesterId);
            if (visitor == null || visitor.CompanyId != conversation.CompanyId)
                return new { messages = Array.Empty<object>(), totalCount = 0 };
        }
        else
        {
            if (conversation.VisitorId != requesterId)
                return new { messages = Array.Empty<object>(), totalCount = 0 };
        }

        var query = _messageService.GetMessagesByConversationId(conversationId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.Content,
                m.SenderTypeId,
                SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                SenderAvatar = m.Sender.AvatarUrl,
                m.IsRead,
                m.ReadAt,
                m.CreatedAt
            })
            .ToListAsync();

        return new
        {
            messages,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            conversation = new
            {
                conversation.Id,
                conversation.Subject,
                conversation.VisitorId,
                conversation.CompanyId,
                conversation.TourId,
                conversation.IsClosedByCompany,
                conversation.IsClosedByVisitor
            }
        };
    }

    public async Task<int> GetUnreadCountForVisitorAsync(int visitorId)
        => await _messageService.GetUnreadCountForVisitorAsync(visitorId);

    public async Task<int> GetUnreadCountForCompanyAsync(int companyId)
        => await _messageService.GetUnreadCountForCompanyAsync(companyId);

    public async Task<(bool success, string message, object? data)> CreateConversationAsync(
        int visitorId, int companyId, int? tourId, string subject, string firstMessage)
    {
        var company = await _companyService.GetByIdAsync(companyId);
        if (company == null)
            return (false, "Error.CompanyNotFound", null);

        var conversation = new Conversation
        {
            VisitorId = visitorId,
            CompanyId = companyId,
            TourId = tourId,
            Subject = subject,
            LastMessageAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _messageService.AddConversation(conversation);
        await _unitOfWork.SaveChangesAsync();

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = visitorId,
            SenderTypeId = MessageSenderTypes.Ids.Visitor,
            Content = firstMessage,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _messageService.AddMessage(message);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Success.ConversationCreated", new { conversationId = conversation.Id, messageId = message.Id });
    }

    public async Task<(bool success, string message, object? data)> SendMessageAsync(
        int conversationId, int senderId, int senderTypeId, string content)
    {
        var conversation = await _messageService.GetConversationByIdAsync(conversationId);
        if (conversation == null)
            return (false, "Error.ConversationNotFound", null);

        // Kapalı konuşma kontrolü
        if (senderTypeId == MessageSenderTypes.Ids.Visitor && conversation.IsClosedByVisitor)
            return (false, "Error.ConversationClosed", null);
        if (senderTypeId == MessageSenderTypes.Ids.Company && conversation.IsClosedByCompany)
            return (false, "Error.ConversationClosed", null);

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            SenderTypeId = senderTypeId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _messageService.AddMessage(message);

        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Success.MessageSent", new { messageId = message.Id });
    }

    public async Task<(bool success, string message)> MarkAsReadAsync(int conversationId, int requesterId, bool isCompany)
    {
        var conversation = await _messageService.GetConversationByIdAsync(conversationId);
        if (conversation == null)
            return (false, "Error.ConversationNotFound");

        // Karsi tarafin mesajlarini okundu isaretle
        var targetSenderType = isCompany ? MessageSenderTypes.Ids.Visitor : MessageSenderTypes.Ids.Company;

        var unreadMessages = await _messageService.GetMessagesByConversationId(conversationId)
            .Where(m => !m.IsRead && m.SenderTypeId == targetSenderType)
            .ToListAsync();

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
            msg.ReadAt = DateTime.UtcNow;
            msg.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.MessagesMarkedAsRead");
    }
}
