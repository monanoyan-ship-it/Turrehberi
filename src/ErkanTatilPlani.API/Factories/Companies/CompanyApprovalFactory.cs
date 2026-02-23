using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Companies;
using ErkanTatilPlani.Core.Infrastructure;

namespace ErkanTatilPlani.API.Factories.Companies;

public class CompanyApprovalFactory : ICompanyApprovalFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyApprovalFactory(ICompanyEntityService companyService, IUnitOfWork unitOfWork)
    {
        _companyService = companyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Company>> GetPendingCompaniesAsync()
    {
        return await _companyService.GetPendingCompaniesAsync();
    }

    public async Task<(bool success, object? result, int statusCode)> ApproveCompanyAsync(int id, int? reviewedById, string? reviewNotes)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Pending)
            return (false, new { message = "Error.OnlyPendingCanBeApproved" }, 400);

        company.StatusId = CompanyStatuses.Ids.Approved;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.ReviewNotes = reviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.CompanyApproved", company }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> RejectCompanyAsync(int id, int? reviewedById, string rejectionReason, string? reviewNotes)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Pending)
            return (false, new { message = "Error.OnlyPendingCanBeRejected" }, 400);

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return (false, new { message = "Validation.RejectionReasonRequired" }, 400);

        company.StatusId = CompanyStatuses.Ids.Rejected;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.RejectionReason = rejectionReason;
        company.ReviewNotes = reviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.CompanyRejected", company }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> SuspendCompanyAsync(int id, int? reviewedById, string reason)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, new { message = "Error.OnlyApprovedCanBeSuspended" }, 400);

        if (string.IsNullOrWhiteSpace(reason))
            return (false, new { message = "Validation.SuspensionReasonRequired" }, 400);

        company.StatusId = CompanyStatuses.Ids.Suspended;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.ReviewNotes = reason;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.CompanySuspended", company }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> ReactivateCompanyAsync(int id, int? reviewedById, string? reviewNotes)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Suspended)
            return (false, new { message = "Error.OnlySuspendedCanBeReactivated" }, 400);

        company.StatusId = CompanyStatuses.Ids.Approved;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.ReviewNotes = reviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.CompanyReactivated", company }, 200);
    }
}
