using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Support;

public interface ISupportTicketFactory
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<SupportTicket?> GetByIdAsync(int id);
    Task<SupportTicket> CreateAsync(SupportTicket ticket);
    Task<(bool success, string message, int statusCode)> ReplyAsync(int id, string reply);
    Task<(bool success, string message, int statusCode)> MarkReadAsync(int id);
    Task<(bool success, string message, int statusCode)> DeleteAsync(int id);
    Task<object> GetStatsAsync();
}
