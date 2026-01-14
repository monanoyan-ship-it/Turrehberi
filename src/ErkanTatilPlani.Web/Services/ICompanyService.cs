using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public interface ICompanyService
{
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<Company?> CreateAsync(Company company);
    Task<bool> UpdateAsync(int id, Company company);
    Task<bool> DeleteAsync(int id);
}
