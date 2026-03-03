using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Support;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Support;

public class SupportTicketFactory : ISupportTicketFactory
{
    private readonly ISupportTicketEntityService _service;
    private readonly IUnitOfWork _unitOfWork;

    public SupportTicketFactory(ISupportTicketEntityService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await _service.GetAll()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Email,
                t.Subject,
                t.Message,
                t.IsRead,
                t.AdminReply,
                t.RepliedAt,
                t.VisitorId,
                t.CreatedAt
            })
            .ToListAsync<object>();
    }

    public async Task<SupportTicket?> GetByIdAsync(int id)
        => await _service.GetByIdAsync(id);

    public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        _service.Add(ticket);
        await _unitOfWork.SaveChangesAsync();
        return ticket;
    }

    public async Task<(bool success, string message, int statusCode)> ReplyAsync(int id, string reply)
    {
        var ticket = await _service.GetByIdAsync(id);
        if (ticket == null)
            return (false, "Error.TicketNotFound", 404);

        ticket.AdminReply = reply;
        ticket.RepliedAt = DateTime.UtcNow;
        ticket.IsRead = true;
        ticket.UpdatedAt = DateTime.UtcNow;
        _service.Update(ticket);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.Replied", 200);
    }

    public async Task<(bool success, string message, int statusCode)> MarkReadAsync(int id)
    {
        var ticket = await _service.GetByIdAsync(id);
        if (ticket == null)
            return (false, "Error.TicketNotFound", 404);

        ticket.IsRead = true;
        ticket.UpdatedAt = DateTime.UtcNow;
        _service.Update(ticket);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.Updated", 200);
    }

    public async Task<(bool success, string message, int statusCode)> DeleteAsync(int id)
    {
        var ticket = await _service.GetByIdAsync(id);
        if (ticket == null)
            return (false, "Error.TicketNotFound", 404);

        ticket.IsActive = false;
        ticket.UpdatedAt = DateTime.UtcNow;
        _service.Update(ticket);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.Deleted", 200);
    }

    public async Task<object> GetStatsAsync()
    {
        var total = await _service.GetAll().CountAsync();
        var unread = await _service.GetAll().CountAsync(t => !t.IsRead);
        var replied = await _service.GetAll().CountAsync(t => t.AdminReply != null);
        return new { total, unread, replied };
    }
}
