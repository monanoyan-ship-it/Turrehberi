using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IFavoriteEntityService
{
    IQueryable<FavoriteTour> GetByVisitorId(int visitorId);
    Task<FavoriteTour?> GetByVisitorAndTourAsync(int visitorId, int tourId);
    Task<bool> IsFavoriteAsync(int visitorId, int tourId);
    Task<List<int>> GetFavoriteTourIdsAsync(int visitorId, int[] tourIds);
    void Add(FavoriteTour favorite);
}
