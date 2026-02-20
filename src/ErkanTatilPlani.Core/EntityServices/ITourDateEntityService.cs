using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourDateEntityService
{
    IQueryable<TourDate> GetByTourId(int tourId);
    IQueryable<TourDate> GetAvailableDates(int tourId);
    Task<TourDate?> GetByIdAsync(int id);
    IQueryable<TourDate> GetByTourIds(IEnumerable<int> tourIds);
    void Add(TourDate tourDate);
    void Update(TourDate tourDate);
}
