using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class MessageEntityService : IMessageEntityService
{
    private readonly AppDbContext _context;

    public MessageEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Conversation> GetConversationsByVisitorId(int visitorId)
        => _context.Conversations.Where(c => c.VisitorId == visitorId && c.IsActive);

    public IQueryable<Conversation> GetConversationsByCompanyId(int companyId)
        => _context.Conversations.Where(c => c.CompanyId == companyId && c.IsActive);

    public async Task<Conversation?> GetConversationByIdAsync(int id)
        => await _context.Conversations.FindAsync(id);

    public IQueryable<Message> GetMessagesByConversationId(int conversationId)
        => _context.Messages.Where(m => m.ConversationId == conversationId && m.IsActive);

    public async Task<int> GetUnreadCountForVisitorAsync(int visitorId)
        => await _context.Messages
            .Where(m => m.Conversation.VisitorId == visitorId
                && m.Conversation.IsActive
                && m.IsActive
                && !m.IsRead
                && m.SenderTypeId == MessageSenderTypes.Ids.Company)
            .CountAsync();

    public async Task<int> GetUnreadCountForCompanyAsync(int companyId)
        => await _context.Messages
            .Where(m => m.Conversation.CompanyId == companyId
                && m.Conversation.IsActive
                && m.IsActive
                && !m.IsRead
                && m.SenderTypeId == MessageSenderTypes.Ids.Visitor)
            .CountAsync();

    public void AddConversation(Conversation conversation) => _context.Conversations.Add(conversation);
    public void AddMessage(Message message) => _context.Messages.Add(message);
}
