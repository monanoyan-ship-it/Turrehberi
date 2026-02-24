using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourScheduleEntityService
{
    IQueryable<TourSchedule> GetByTourId(int tourId);
    IQueryable<TourSchedule> GetActiveByTourId(int tourId);
    Task<TourSchedule?> GetByIdAsync(int id);
    void Add(TourSchedule schedule);
    void Update(TourSchedule schedule);
}
