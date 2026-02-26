using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TripStoryPhotoEntityService : ITripStoryPhotoEntityService
{
    private readonly AppDbContext _context;

    public TripStoryPhotoEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TripStoryPhoto> GetByStoryId(int storyId)
        => _context.TripStoryPhotos.Where(p => p.TripStoryId == storyId && p.IsActive);

    public async Task<TripStoryPhoto?> GetByIdAsync(int id)
        => await _context.TripStoryPhotos.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public void Add(TripStoryPhoto photo) => _context.TripStoryPhotos.Add(photo);

    public void Remove(TripStoryPhoto photo)
    {
        photo.IsActive = false;
        photo.UpdatedAt = DateTime.UtcNow;
    }
}
