using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourDigitalContentEntityService
{
    IQueryable<TourDigitalContent> GetActiveContents();
    IQueryable<TourDigitalContent> GetByTourId(int tourId);
    Task<TourDigitalContent?> GetByIdAsync(int id);
    void Add(TourDigitalContent content);
    void Update(TourDigitalContent content);
}
