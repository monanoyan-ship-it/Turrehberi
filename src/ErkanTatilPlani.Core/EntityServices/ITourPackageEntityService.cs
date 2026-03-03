using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourPackageEntityService
{
    IQueryable<TourPackage> GetActivePackages();
    Task<TourPackage?> GetByIdAsync(int id);
    IQueryable<TourPackage> GetByVisitorId(int visitorId);
    void Add(TourPackage package);
    void Update(TourPackage package);
    // Items
    void AddItem(TourPackageItem item);
    void RemoveItem(TourPackageItem item);
    Task<TourPackageItem?> GetItemByIdAsync(int itemId);
}
