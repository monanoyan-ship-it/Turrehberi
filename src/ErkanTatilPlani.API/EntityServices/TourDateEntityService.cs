using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourDateEntityService : ITourDateEntityService
{
    private readonly AppDbContext _context;

    public TourDateEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TourDate> GetByTourId(int tourId)
        => _context.TourDates.Where(td => td.TourId == tourId && td.IsActive);

    public IQueryable<TourDate> GetAvailableDates(int tourId)
        => _context.TourDates.Where(td => td.TourId == tourId && td.IsActive && td.IsAvailable && td.StartDate >= DateTime.UtcNow);

    public async Task<TourDate?> GetByIdAsync(int id)
        => await _context.TourDates.FindAsync(id);

    public IQueryable<TourDate> GetByTourIds(IEnumerable<int> tourIds)
        => _context.TourDates.Where(td => tourIds.Contains(td.TourId) && td.IsActive);

    public void Add(TourDate tourDate) => _context.TourDates.Add(tourDate);

    public void Update(TourDate tourDate) => _context.Entry(tourDate).State = EntityState.Modified;
}
