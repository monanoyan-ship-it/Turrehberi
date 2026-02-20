namespace ErkanTatilPlani.Core.Factories.TourWatches;

public interface ITourWatchFactory
{
    Task<object> GetWatchedToursAsync(int visitorId);
    Task<bool> CheckWatchAsync(int visitorId, int tourId);
    Task<List<int>> CheckMultipleWatchesAsync(int visitorId, int[] tourIds);
    Task<(bool success, string message)> AddWatchAsync(int visitorId, int tourId, int watchDays);
    Task<(bool success, string message)> RemoveWatchAsync(int visitorId, int tourId);
    Task<(bool isWatching, string message, bool tourNotFound)> ToggleWatchAsync(int visitorId, int tourId, int watchDays);
}
