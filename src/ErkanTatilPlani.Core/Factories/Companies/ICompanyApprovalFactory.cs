using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Companies;

public interface ICompanyApprovalFactory
{
    Task<IEnumerable<Company>> GetPendingCompaniesAsync();
    Task<(bool success, object? result, int statusCode)> ApproveCompanyAsync(int id, int? reviewedById, string? reviewNotes);
    Task<(bool success, object? result, int statusCode)> RejectCompanyAsync(int id, int? reviewedById, string rejectionReason, string? reviewNotes);
    Task<(bool success, object? result, int statusCode)> SuspendCompanyAsync(int id, int? reviewedById, string reason);
    Task<(bool success, object? result, int statusCode)> ReactivateCompanyAsync(int id, int? reviewedById, string? reviewNotes);
}
