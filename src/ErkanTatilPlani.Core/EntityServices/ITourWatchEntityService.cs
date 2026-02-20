using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourWatchEntityService
{
    IQueryable<TourWatch> GetByVisitorId(int visitorId);
    Task<TourWatch?> GetByVisitorAndTourAsync(int visitorId, int tourId);
    Task<List<TourWatch>> GetActiveWatchersForTourAsync(int tourId);
    Task<List<int>> GetWatchedTourIdsAsync(int visitorId, int[] tourIds);
    void Add(TourWatch watch);
}
