namespace ErkanTatilPlani.Core.Factories.Messages;

public interface IMessageFactory
{
    Task<object> GetConversationsForVisitorAsync(int visitorId, int page, int pageSize);
    Task<object> GetConversationsForCompanyAsync(int companyId, int page, int pageSize);
    Task<object> GetMessagesAsync(int conversationId, int requesterId, bool isCompany, int page, int pageSize);
    Task<int> GetUnreadCountForVisitorAsync(int visitorId);
    Task<int> GetUnreadCountForCompanyAsync(int companyId);
    Task<(bool success, string message, object? data)> CreateConversationAsync(int visitorId, int companyId, int? tourId, string subject, string firstMessage);
    Task<(bool success, string message, object? data)> SendMessageAsync(int conversationId, int senderId, int senderTypeId, string content);
    Task<(bool success, string message)> MarkAsReadAsync(int conversationId, int requesterId, bool isCompany);
}
