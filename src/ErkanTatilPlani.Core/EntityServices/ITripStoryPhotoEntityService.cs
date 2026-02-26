using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITripStoryPhotoEntityService
{
    IQueryable<TripStoryPhoto> GetByStoryId(int storyId);
    Task<TripStoryPhoto?> GetByIdAsync(int id);
    void Add(TripStoryPhoto photo);
    void Remove(TripStoryPhoto photo);
}
