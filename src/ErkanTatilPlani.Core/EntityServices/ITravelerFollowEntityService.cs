using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITravelerFollowEntityService
{
    IQueryable<TravelerFollow> GetByFollowerId(int followerId);
    IQueryable<TravelerFollow> GetByFollowedId(int followedId);
    Task<TravelerFollow?> GetByPairAsync(int followerId, int followedId);
    void Add(TravelerFollow follow);
    void Remove(TravelerFollow follow);
}
