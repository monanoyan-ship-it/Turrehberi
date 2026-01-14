using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public interface ITourService
{
    Task<IEnumerable<Tour>> GetAllAsync();
    Task<Tour?> GetByIdAsync(int id);
    Task<Tour?> CreateAsync(Tour tour);
    Task<bool> UpdateAsync(int id, Tour tour);
    Task<bool> DeleteAsync(int id);
}
