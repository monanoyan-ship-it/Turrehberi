using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourWatchEntityService : ITourWatchEntityService
{
    private readonly AppDbContext _context;

    public TourWatchEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TourWatch> GetByVisitorId(int visitorId)
        => _context.TourWatches.Where(w => w.VisitorId == visitorId && w.IsActive);

    public async Task<TourWatch?> GetByVisitorAndTourAsync(int visitorId, int tourId)
        => await _context.TourWatches.FirstOrDefaultAsync(w => w.VisitorId == visitorId && w.TourId == tourId);

    public async Task<List<TourWatch>> GetActiveWatchersForTourAsync(int tourId)
        => await _context.TourWatches
            .Where(w => w.TourId == tourId && w.IsActive && w.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

    public async Task<List<int>> GetWatchedTourIdsAsync(int visitorId, int[] tourIds)
        => await _context.TourWatches
            .Where(w => w.VisitorId == visitorId && tourIds.Contains(w.TourId) && w.IsActive && w.ExpiresAt > DateTime.UtcNow)
            .Select(w => w.TourId)
            .ToListAsync();

    public void Add(TourWatch watch) => _context.TourWatches.Add(watch);
}
