using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourPhotoEntityService
{
    Task<int> GetPhotoCountAsync(int tourId);
    Task<TourPhoto?> GetByIdAsync(int photoId, int tourId);
    IQueryable<TourPhoto> GetActivePhotos(int tourId);
    Task<List<TourPhoto>> GetByIdsAsync(int tourId, List<int> photoIds);
    void Add(TourPhoto photo);
    void Update(TourPhoto photo);
    Task ClearCoverAsync(int tourId);
    Task ClearCoverExceptAsync(int tourId, int photoId);
}
