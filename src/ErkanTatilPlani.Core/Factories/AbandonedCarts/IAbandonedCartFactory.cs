namespace ErkanTatilPlani.Core.Factories.AbandonedCarts;

public interface IAbandonedCartFactory
{
    Task TrackCartAsync(int? visitorId, string email, int tourId, int? scheduleId, int numberOfPeople, decimal price, string? dateToken);
    Task MarkRecoveredAsync(int visitorId, int tourId, int reservationId);
    Task ProcessAbandonedCartsAsync();
}
