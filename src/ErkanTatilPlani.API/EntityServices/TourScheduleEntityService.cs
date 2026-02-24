using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourScheduleEntityService : ITourScheduleEntityService
{
    private readonly AppDbContext _context;

    public TourScheduleEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TourSchedule> GetByTourId(int tourId)
        => _context.TourSchedules.Where(s => s.TourId == tourId);

    public IQueryable<TourSchedule> GetActiveByTourId(int tourId)
        => _context.TourSchedules.Where(s => s.TourId == tourId && s.IsActive);

    public async Task<TourSchedule?> GetByIdAsync(int id)
        => await _context.TourSchedules.FindAsync(id);

    public void Add(TourSchedule schedule) => _context.TourSchedules.Add(schedule);

    public void Update(TourSchedule schedule) => _context.Entry(schedule).State = EntityState.Modified;
}
