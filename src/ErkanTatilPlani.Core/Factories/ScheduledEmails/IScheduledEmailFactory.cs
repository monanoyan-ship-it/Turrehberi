namespace ErkanTatilPlani.Core.Factories.ScheduledEmails;

public interface IScheduledEmailFactory
{
    Task QueueReservationEmailsAsync(int reservationId);
    Task CancelReservationEmailsAsync(int reservationId);
    Task ProcessPendingEmailsAsync();
}
