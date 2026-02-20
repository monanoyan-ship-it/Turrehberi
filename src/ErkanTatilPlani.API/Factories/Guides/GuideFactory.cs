using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Guides;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Guides;

public class GuideFactory : IGuideFactory
{
    private readonly IGuideEntityService _guideService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ITourDateEntityService _tourDateService;
    private readonly IUnitOfWork _unitOfWork;

    public GuideFactory(
        IGuideEntityService guideService,
        IVisitorEntityService visitorService,
        ITourDateEntityService tourDateService,
        IUnitOfWork unitOfWork)
    {
        _guideService = guideService;
        _visitorService = visitorService;
        _tourDateService = tourDateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object result, int statusCode)> GetCompanyGuidesAsync(int visitorId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guides = await _guideService.GetByCompanyId(check.companyId!.Value)
            .OrderBy(g => g.FirstName).ThenBy(g => g.LastName)
            .Select(g => new
            {
                g.Id, g.FirstName, g.LastName, g.Phone, g.Email, g.PhotoUrl,
                g.Languages, g.Bio, g.ExperienceYears,
                g.TotalToursCompleted, g.AverageRating,
                ActiveAssignments = g.Assignments.Count(a => a.IsActive && a.TourDate.StartDate >= DateTime.UtcNow)
            })
            .ToListAsync();

        return (true, guides, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetGuideByIdAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdWithAssignmentsAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Rehber bulunamadi" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Bu rehber firmaniza ait degil" }, 403);

        var assignments = guide.Assignments
            .OrderByDescending(a => a.TourDate.StartDate)
            .Select(a => new
            {
                a.Id, a.TourDateId, a.StatusId,
                StatusName = GuideAssignmentStatuses.GetById(a.StatusId)?.SystemName,
                StatusCss = GuideAssignmentStatuses.GetById(a.StatusId)?.CssClass,
                a.Notes,
                TourName = a.TourDate.Tour.Name,
                a.TourDate.StartDate, a.TourDate.EndDate
            });

        return (true, new
        {
            guide.Id, guide.FirstName, guide.LastName, guide.Phone, guide.Email, guide.PhotoUrl,
            guide.Languages, guide.Bio, guide.ExperienceYears,
            guide.TotalToursCompleted, guide.AverageRating,
            Assignments = assignments
        }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> CreateGuideAsync(int visitorId, Guide guide)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        guide.CompanyId = check.companyId!.Value;
        _guideService.Add(guide);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { guide.Id, message = "Rehber olusturuldu" }, 201);
    }

    public async Task<(bool success, object result, int statusCode)> UpdateGuideAsync(int visitorId, int guideId, Guide guideData)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Rehber bulunamadi" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Bu rehber firmaniza ait degil" }, 403);

        guide.FirstName = guideData.FirstName;
        guide.LastName = guideData.LastName;
        guide.Phone = guideData.Phone;
        guide.Email = guideData.Email;
        guide.PhotoUrl = guideData.PhotoUrl;
        guide.Languages = guideData.Languages;
        guide.Bio = guideData.Bio;
        guide.ExperienceYears = guideData.ExperienceYears;
        guide.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, new { message = "Rehber guncellendi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeleteGuideAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Rehber bulunamadi" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Bu rehber firmaniza ait degil" }, 403);

        guide.IsActive = false;
        guide.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Rehber silindi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> AssignGuideToDateAsync(int visitorId, int guideId, int tourDateId, string? notes)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Rehber bulunamadi" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Bu rehber firmaniza ait degil" }, 403);

        var tourDate = await _tourDateService.GetByIdAsync(tourDateId);
        if (tourDate == null || !tourDate.IsActive) return (false, new { message = "Tur tarihi bulunamadi" }, 404);

        var existing = await _guideService.GetAssignmentsByGuideId(guideId)
            .FirstOrDefaultAsync(a => a.TourDateId == tourDateId);
        if (existing != null) return (false, new { message = "Rehber bu tarihe zaten atanmis" }, 400);

        var assignment = new TourGuideAssignment
        {
            GuideId = guideId,
            TourDateId = tourDateId,
            StatusId = GuideAssignmentStatuses.Ids.Confirmed,
            Notes = notes
        };

        _guideService.AddAssignment(assignment);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { assignment.Id, message = "Rehber atandi" }, 201);
    }

    public async Task<(bool success, object result, int statusCode)> RemoveAssignmentAsync(int visitorId, int assignmentId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var assignment = await _guideService.GetAssignmentByIdAsync(assignmentId);
        if (assignment == null || !assignment.IsActive) return (false, new { message = "Atama bulunamadi" }, 404);
        if (assignment.Guide.CompanyId != check.companyId) return (false, new { message = "Bu atama firmaniza ait degil" }, 403);

        _guideService.RemoveAssignment(assignment);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Atama kaldirildi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetGuideAvailabilityAsync(int visitorId, int guideId, int year, int month)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Rehber bulunamadi" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Bu rehber firmaniza ait degil" }, 403);

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var assignments = await _guideService.GetAssignmentsByGuideId(guideId)
            .Where(a => a.TourDate.StartDate >= monthStart && a.TourDate.StartDate < monthEnd)
            .Select(a => new
            {
                a.Id, a.TourDateId, a.StatusId,
                TourName = a.TourDate.Tour.Name,
                a.TourDate.StartDate, a.TourDate.EndDate
            })
            .OrderBy(a => a.StartDate)
            .ToListAsync();

        return (true, new { guideId, year, month, assignments }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetGuidePerformanceAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Rehber bulunamadi" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Bu rehber firmaniza ait degil" }, 403);

        var totalAssignments = await _guideService.GetAssignmentsByGuideId(guideId).CountAsync();
        var completedAssignments = await _guideService.GetAssignmentsByGuideId(guideId)
            .CountAsync(a => a.TourDate.EndDate < DateTime.UtcNow);
        var upcomingAssignments = await _guideService.GetAssignmentsByGuideId(guideId)
            .CountAsync(a => a.TourDate.StartDate >= DateTime.UtcNow);

        return (true, new
        {
            guideId,
            guide.TotalToursCompleted,
            guide.AverageRating,
            totalAssignments,
            completedAssignments,
            upcomingAssignments
        }, 200);
    }

    private async Task<(string? errorMessage, int statusCode, int? companyId)> CheckCompanyOwnership(int visitorId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return ("Kullanici bulunamadi", 401, null);
        if (visitor.Company == null) return ("Firma sahibi degilsiniz", 403, null);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return ("Firma durumu uygun degil", 403, null);

        return (null, 200, visitor.Company.Id);
    }
}
