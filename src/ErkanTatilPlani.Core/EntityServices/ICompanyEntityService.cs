using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ICompanyEntityService
{
    Task<IEnumerable<Company>> GetActiveCompaniesAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<Company?> GetBySlugAsync(string slug);
    Task<Company?> GetByTaxNumberAsync(string taxNumber);
    IQueryable<Company> GetActiveApprovedCompanies();
    Task<IEnumerable<Company>> GetPendingCompaniesAsync();
    void Add(Company company);
    void Update(Company company);
}
