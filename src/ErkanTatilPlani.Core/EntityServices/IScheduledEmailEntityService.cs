using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IScheduledEmailEntityService
{
    IQueryable<ScheduledEmail> GetPendingEmails(DateTime before);
    IQueryable<ScheduledEmail> GetByReservationId(int reservationId);
    void Add(ScheduledEmail email);
    void Update(ScheduledEmail email);
}
