using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourPackageEntityService : ITourPackageEntityService
{
    private readonly AppDbContext _db;

    public TourPackageEntityService(AppDbContext db)
    {
        _db = db;
    }

    public IQueryable<TourPackage> GetActivePackages()
        => _db.TourPackages.Where(p => p.IsActive);

    public async Task<TourPackage?> GetByIdAsync(int id)
        => await _db.TourPackages
            .Include(p => p.Items).ThenInclude(i => i.Tour)
            .Include(p => p.Visitor)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public IQueryable<TourPackage> GetByVisitorId(int visitorId)
        => _db.TourPackages.Where(p => p.VisitorId == visitorId && p.IsActive);

    public void Add(TourPackage package) => _db.TourPackages.Add(package);
    public void Update(TourPackage package) => _db.TourPackages.Update(package);

    public void AddItem(TourPackageItem item) => _db.TourPackageItems.Add(item);
    public void RemoveItem(TourPackageItem item) => _db.TourPackageItems.Remove(item);
    public async Task<TourPackageItem?> GetItemByIdAsync(int itemId)
        => await _db.TourPackageItems.FirstOrDefaultAsync(i => i.Id == itemId);
}
