using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class SupportTicketEntityService : ISupportTicketEntityService
{
    private readonly AppDbContext _context;

    public SupportTicketEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<SupportTicket> GetAll()
        => _context.SupportTickets.Where(t => t.IsActive);

    public async Task<SupportTicket?> GetByIdAsync(int id)
        => await _context.SupportTickets.FindAsync(id);

    public void Add(SupportTicket ticket) => _context.SupportTickets.Add(ticket);

    public void Update(SupportTicket ticket)
        => _context.Entry(ticket).State = EntityState.Modified;
}
