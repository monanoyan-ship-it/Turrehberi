using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Companies;

public interface ICompanyPageFactory
{
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetPagesAsync(int visitorId, int companyId);
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetPageAsync(int visitorId, int companyId, int id);
    Task<(CompanyPage? page, string? errorMessage, string? errorCode, int? statusCode)> CreatePageAsync(int visitorId, int companyId, CompanyPage page);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdatePageAsync(int visitorId, int companyId, int id, CompanyPage page);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> DeletePageAsync(int visitorId, int companyId, int id);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> ReorderPagesAsync(int visitorId, int companyId, List<int> pageIds);
    Task<object?> GetPublicPagesAsync(string companySlug);
    Task<object?> GetPublicPageBySlugAsync(string companySlug, string pageSlug);
}
