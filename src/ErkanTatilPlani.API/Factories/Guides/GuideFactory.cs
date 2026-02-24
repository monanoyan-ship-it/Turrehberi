using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Guides;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Guides;

public class GuideFactory : IGuideFactory
{
    private readonly IGuideEntityService _guideService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ITourScheduleEntityService _scheduleService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUnitOfWork _unitOfWork;

    public GuideFactory(
        IGuideEntityService guideService,
        IVisitorEntityService visitorService,
        ITourScheduleEntityService scheduleService,
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork)
    {
        _guideService = guideService;
        _visitorService = visitorService;
        _scheduleService = scheduleService;
        _fileUploadService = fileUploadService;
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
                ActiveAssignments = g.Assignments.Count(a => a.IsActive)
            })
            .ToListAsync();

        return (true, guides, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetGuideByIdAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdWithAssignmentsAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        var assignments = guide.Assignments
            .OrderByDescending(a => a.Schedule.ValidFrom)
            .Select(a => new
            {
                a.Id, a.ScheduleId, a.StatusId,
                StatusName = GuideAssignmentStatuses.GetById(a.StatusId)?.SystemName,
                StatusCss = GuideAssignmentStatuses.GetById(a.StatusId)?.CssClass,
                a.Notes,
                TourName = a.Schedule.Tour.Name,
                StartTime = a.Schedule.StartTime.ToString(@"hh\:mm"),
                a.Schedule.ValidFrom,
                a.Schedule.ValidTo
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

        return (true, new { guide.Id, message = "Success.GuideCreated" }, 201);
    }

    public async Task<(bool success, object result, int statusCode)> UpdateGuideAsync(int visitorId, int guideId, Guide guideData)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

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
        return (true, new { message = "Success.GuideUpdated" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeleteGuideAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        guide.IsActive = false;
        guide.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.GuideDeleted" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> AssignGuideToDateAsync(int visitorId, int guideId, int tourDateId, string? notes)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        // tourDateId artik scheduleId olarak kullaniliyor
        var schedule = await _scheduleService.GetByIdAsync(tourDateId);
        if (schedule == null || !schedule.IsActive) return (false, new { message = "Error.ScheduleNotFound" }, 404);

        var existing = await _guideService.GetAssignmentsByGuideId(guideId)
            .FirstOrDefaultAsync(a => a.ScheduleId == tourDateId);
        if (existing != null) return (false, new { message = "Error.GuideAlreadyAssigned" }, 400);

        var assignment = new TourGuideAssignment
        {
            GuideId = guideId,
            ScheduleId = tourDateId,
            StatusId = GuideAssignmentStatuses.Ids.Confirmed,
            Notes = notes
        };

        _guideService.AddAssignment(assignment);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { assignment.Id, message = "Success.GuideAssigned" }, 201);
    }

    public async Task<(bool success, object result, int statusCode)> RemoveAssignmentAsync(int visitorId, int assignmentId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var assignment = await _guideService.GetAssignmentByIdAsync(assignmentId);
        if (assignment == null || !assignment.IsActive) return (false, new { message = "Error.AssignmentNotFound" }, 404);
        if (assignment.Guide.CompanyId != check.companyId) return (false, new { message = "Error.AssignmentNotOwnedByCompany" }, 403);

        _guideService.RemoveAssignment(assignment);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.AssignmentRemoved" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetGuideAvailabilityAsync(int visitorId, int guideId, int year, int month)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        var assignments = await _guideService.GetAssignmentsByGuideId(guideId)
            .Select(a => new
            {
                a.Id, a.ScheduleId, a.StatusId,
                TourName = a.Schedule.Tour.Name,
                StartTime = a.Schedule.StartTime.ToString(@"hh\:mm"),
                a.Schedule.ValidFrom,
                a.Schedule.ValidTo
            })
            .ToListAsync();

        return (true, new { guideId, year, month, assignments }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetGuidePerformanceAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        var totalAssignments = await _guideService.GetAssignmentsByGuideId(guideId).CountAsync();

        return (true, new
        {
            guideId,
            guide.TotalToursCompleted,
            guide.AverageRating,
            totalAssignments
        }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> UploadGuidePhotoAsync(int visitorId, int guideId, Stream fileStream, string fileName)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        if (fileStream == null || fileStream.Length == 0)
            return (false, new { message = "Error.NoFileSelected" }, 400);

        if (!string.IsNullOrEmpty(guide.PhotoUrl) && guide.PhotoUrl.StartsWith("/uploads/"))
            await _fileUploadService.DeleteFileAsync(guide.PhotoUrl);

        var result = await _fileUploadService.UploadImageAsync(fileStream, fileName, "guides", 400);
        if (!result.Success)
            return (false, new { message = result.ErrorMessage }, 400);

        guide.PhotoUrl = result.Url;
        guide.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.PhotoUploaded", photoUrl = result.Url }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeleteGuidePhotoAsync(int visitorId, int guideId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var guide = await _guideService.GetByIdAsync(guideId);
        if (guide == null || !guide.IsActive) return (false, new { message = "Error.GuideNotFound" }, 404);
        if (guide.CompanyId != check.companyId) return (false, new { message = "Error.GuideNotOwnedByCompany" }, 403);

        if (!string.IsNullOrEmpty(guide.PhotoUrl) && guide.PhotoUrl.StartsWith("/uploads/"))
            await _fileUploadService.DeleteFileAsync(guide.PhotoUrl);

        guide.PhotoUrl = null;
        guide.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.PhotoDeleted" }, 200);
    }

    private async Task<(string? errorMessage, int statusCode, int? companyId)> CheckCompanyOwnership(int visitorId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return ("Error.UserNotFound", 401, null);
        if (visitor.Company == null) return ("Error.NotCompanyOwner", 403, null);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return ("Error.CompanyStatusInvalid", 403, null);

        return (null, 200, visitor.Company.Id);
    }
}
