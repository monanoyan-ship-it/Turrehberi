using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ICompanyPageEntityService
{
    IQueryable<CompanyPage> GetByCompanyId(int companyId);
    Task<CompanyPage?> GetByIdAsync(int id);
    Task<CompanyPage?> GetBySlugAsync(int companyId, string slug);
    void Add(CompanyPage page);
    void Update(CompanyPage page);
}
