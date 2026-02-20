using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourDateEntityService
{
    IQueryable<TourDate> GetByTourId(int tourId);
    IQueryable<TourDate> GetAvailableDates(int tourId);
    Task<TourDate?> GetByIdAsync(int id);
    void Add(TourDate tourDate);
    void Update(TourDate tourDate);
}
