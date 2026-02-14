using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITourEntityService
{
    IQueryable<Tour> GetActiveTours();
    IQueryable<Tour> GetActiveToursWithCompany();
    Task<Tour?> GetByIdAsync(int id);
    Task<Tour?> GetByIdWithCompanyAsync(int id);
    Task<IEnumerable<Tour>> GetByCompanyIdAsync(int companyId);
    void Add(Tour tour);
    void Update(Tour tour);
}
