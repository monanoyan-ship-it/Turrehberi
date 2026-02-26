using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class ScheduledEmailEntityService : IScheduledEmailEntityService
{
    private readonly AppDbContext _context;

    public ScheduledEmailEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<ScheduledEmail> GetPendingEmails(DateTime before)
        => _context.ScheduledEmails.Where(e =>
            e.IsActive &&
            e.StatusId == ScheduledEmailStatuses.Ids.Pending &&
            e.ScheduledAt <= before);

    public IQueryable<ScheduledEmail> GetByReservationId(int reservationId)
        => _context.ScheduledEmails.Where(e => e.ReservationId == reservationId && e.IsActive);

    public void Add(ScheduledEmail email) => _context.ScheduledEmails.Add(email);
    public void Update(ScheduledEmail email) => _context.Entry(email).State = EntityState.Modified;
}
