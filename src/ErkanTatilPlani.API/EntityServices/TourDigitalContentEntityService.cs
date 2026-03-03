using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourDigitalContentEntityService : ITourDigitalContentEntityService
{
    private readonly AppDbContext _db;

    public TourDigitalContentEntityService(AppDbContext db)
    {
        _db = db;
    }

    public IQueryable<TourDigitalContent> GetActiveContents()
        => _db.TourDigitalContents.Where(c => c.IsActive);

    public IQueryable<TourDigitalContent> GetByTourId(int tourId)
        => _db.TourDigitalContents.Where(c => c.TourId == tourId && c.IsActive);

    public async Task<TourDigitalContent?> GetByIdAsync(int id)
        => await _db.TourDigitalContents.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

    public void Add(TourDigitalContent content) => _db.TourDigitalContents.Add(content);
    public void Update(TourDigitalContent content) => _db.TourDigitalContents.Update(content);
}
