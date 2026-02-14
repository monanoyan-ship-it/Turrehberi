using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class FavoriteEntityService : IFavoriteEntityService
{
    private readonly AppDbContext _context;

    public FavoriteEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<FavoriteTour> GetByVisitorId(int visitorId)
        => _context.FavoriteTours.Where(f => f.VisitorId == visitorId && f.IsActive);

    public async Task<FavoriteTour?> GetByVisitorAndTourAsync(int visitorId, int tourId)
        => await _context.FavoriteTours.FirstOrDefaultAsync(f => f.VisitorId == visitorId && f.TourId == tourId);

    public async Task<bool> IsFavoriteAsync(int visitorId, int tourId)
        => await _context.FavoriteTours.AnyAsync(f => f.VisitorId == visitorId && f.TourId == tourId && f.IsActive);

    public async Task<List<int>> GetFavoriteTourIdsAsync(int visitorId, int[] tourIds)
        => await _context.FavoriteTours
            .Where(f => f.VisitorId == visitorId && tourIds.Contains(f.TourId) && f.IsActive)
            .Select(f => f.TourId)
            .ToListAsync();

    public void Add(FavoriteTour favorite) => _context.FavoriteTours.Add(favorite);
}
