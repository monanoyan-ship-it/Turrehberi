using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourPhotoEntityService : ITourPhotoEntityService
{
    private readonly AppDbContext _context;

    public TourPhotoEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetPhotoCountAsync(int tourId)
        => await _context.TourPhotos.CountAsync(p => p.TourId == tourId && p.IsActive);

    public async Task<TourPhoto?> GetByIdAsync(int photoId, int tourId)
        => await _context.TourPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.TourId == tourId);

    public IQueryable<TourPhoto> GetActivePhotos(int tourId)
        => _context.TourPhotos.Where(p => p.TourId == tourId && p.IsActive);

    public async Task<List<TourPhoto>> GetByIdsAsync(int tourId, List<int> photoIds)
        => await _context.TourPhotos.Where(p => p.TourId == tourId && photoIds.Contains(p.Id) && p.IsActive).ToListAsync();

    public void Add(TourPhoto photo) => _context.TourPhotos.Add(photo);

    public void Update(TourPhoto photo) => _context.Entry(photo).State = EntityState.Modified;

    public async Task ClearCoverAsync(int tourId)
        => await _context.TourPhotos.Where(p => p.TourId == tourId && p.IsCover).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsCover, false));

    public async Task ClearCoverExceptAsync(int tourId, int photoId)
        => await _context.TourPhotos.Where(p => p.TourId == tourId && p.IsCover && p.Id != photoId).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsCover, false));
}
