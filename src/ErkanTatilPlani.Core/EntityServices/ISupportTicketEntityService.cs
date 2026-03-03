using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ISupportTicketEntityService
{
    IQueryable<SupportTicket> GetAll();
    Task<SupportTicket?> GetByIdAsync(int id);
    void Add(SupportTicket ticket);
    void Update(SupportTicket ticket);
}
