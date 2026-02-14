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
            return (false, new { message = "Firma bulunamadi" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Pending)
            return (false, new { message = "Sadece bekleyen basvurular onaylanabilir" }, 400);

        company.StatusId = CompanyStatuses.Ids.Approved;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.ReviewNotes = reviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Firma onaylandi", company }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> RejectCompanyAsync(int id, int? reviewedById, string rejectionReason, string? reviewNotes)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Firma bulunamadi" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Pending)
            return (false, new { message = "Sadece bekleyen basvurular reddedilebilir" }, 400);

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return (false, new { message = "Red sebebi zorunludur" }, 400);

        company.StatusId = CompanyStatuses.Ids.Rejected;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.RejectionReason = rejectionReason;
        company.ReviewNotes = reviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Firma basvurusu reddedildi", company }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> SuspendCompanyAsync(int id, int? reviewedById, string reason)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Firma bulunamadi" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, new { message = "Sadece onaylanmis firmalar askiya alinabilir" }, 400);

        if (string.IsNullOrWhiteSpace(reason))
            return (false, new { message = "Askiya alma sebebi zorunludur" }, 400);

        company.StatusId = CompanyStatuses.Ids.Suspended;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.ReviewNotes = reason;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Firma askiya alindi", company }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> ReactivateCompanyAsync(int id, int? reviewedById, string? reviewNotes)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Firma bulunamadi" }, 404);

        if (company.StatusId != CompanyStatuses.Ids.Suspended)
            return (false, new { message = "Sadece askiya alinmis firmalar tekrar aktiflenebilir" }, 400);

        company.StatusId = CompanyStatuses.Ids.Approved;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = reviewedById;
        company.ReviewNotes = reviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Firma tekrar aktiflendi", company }, 200);
    }
}
