using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TravelerFollowEntityService : ITravelerFollowEntityService
{
    private readonly AppDbContext _context;

    public TravelerFollowEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TravelerFollow> GetByFollowerId(int followerId)
        => _context.TravelerFollows.Where(f => f.FollowerId == followerId && f.IsActive);

    public IQueryable<TravelerFollow> GetByFollowedId(int followedId)
        => _context.TravelerFollows.Where(f => f.FollowedId == followedId && f.IsActive);

    public async Task<TravelerFollow?> GetByPairAsync(int followerId, int followedId)
        => await _context.TravelerFollows.FirstOrDefaultAsync(f =>
            f.FollowerId == followerId && f.FollowedId == followedId && f.IsActive);

    public void Add(TravelerFollow follow) => _context.TravelerFollows.Add(follow);

    public void Remove(TravelerFollow follow)
    {
        follow.IsActive = false;
        follow.UpdatedAt = DateTime.UtcNow;
    }
}
