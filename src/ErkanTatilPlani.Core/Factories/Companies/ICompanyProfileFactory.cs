using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Companies;

public interface ICompanyProfileFactory
{
    Task<(bool found, object? result)> GetCompanyProfileAsync(string slug);
    Task<object> GetPublicCompaniesAsync(string? city, string? search, string? sort);
    Task<IEnumerable<Tour>> GetCompanyToursAsync(int companyId);
}
