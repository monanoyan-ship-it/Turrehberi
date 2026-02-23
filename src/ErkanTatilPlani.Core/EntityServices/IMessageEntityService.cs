using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IMessageEntityService
{
    IQueryable<Conversation> GetConversationsByVisitorId(int visitorId);
    IQueryable<Conversation> GetConversationsByCompanyId(int companyId);
    Task<Conversation?> GetConversationByIdAsync(int id);
    IQueryable<Message> GetMessagesByConversationId(int conversationId);
    Task<int> GetUnreadCountForVisitorAsync(int visitorId);
    Task<int> GetUnreadCountForCompanyAsync(int companyId);
    void AddConversation(Conversation conversation);
    void AddMessage(Message message);
}
