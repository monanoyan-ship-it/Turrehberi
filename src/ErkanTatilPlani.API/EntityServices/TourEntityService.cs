using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TourEntityService : ITourEntityService
{
    private readonly AppDbContext _context;

    public TourEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Tour> GetActiveTours()
        => _context.Tours.Where(t => t.IsActive);

    public IQueryable<Tour> GetActiveToursWithCompany()
        => _context.Tours.Include(t => t.Company).Where(t => t.IsActive);

    public async Task<Tour?> GetByIdAsync(int id)
        => await _context.Tours.FindAsync(id);

    public async Task<Tour?> GetByIdWithCompanyAsync(int id)
        => await _context.Tours.Include(t => t.Company).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Tour>> GetByCompanyIdAsync(int companyId)
        => await _context.Tours.Where(t => t.CompanyId == companyId && t.IsActive).ToListAsync();

    public void Add(Tour tour) => _context.Tours.Add(tour);

    public void Update(Tour tour) => _context.Entry(tour).State = EntityState.Modified;
}
