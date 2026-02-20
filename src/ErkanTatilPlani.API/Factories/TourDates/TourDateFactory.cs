using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.TourDates;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.TourDates;

public class TourDateFactory : ITourDateFactory
{
    private readonly ITourDateEntityService _tourDateService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public TourDateFactory(
        ITourDateEntityService tourDateService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _tourDateService = tourDateService;
        _tourService = tourService;
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetTourDatesAsync(int tourId)
    {
        return await _tourDateService.GetAvailableDates(tourId)
            .OrderBy(td => td.StartDate)
            .Select(td => new
            {
                td.Id, td.TourId, td.StartDate, td.EndDate,
                td.Price, td.MaxCapacity, td.BookedCount, td.IsAvailable
            })
            .ToListAsync();
    }

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> ManageTourDatesAsync(int visitorId, int tourId)
    {
        var check = await CheckTourOwnership(visitorId, tourId);
        if (check.errorMessage != null) return (null, check.errorMessage, check.errorCode, check.statusCode);

        var dates = await _tourDateService.GetByTourId(tourId)
            .OrderBy(td => td.StartDate)
            .Select(td => new
            {
                td.Id, td.TourId, td.StartDate, td.EndDate,
                td.Price, td.MaxCapacity, td.BookedCount, td.IsAvailable,
                td.CreatedAt
            })
            .ToListAsync();

        return (dates, null, null, null);
    }

    public async Task<(TourDate? tourDate, string? errorMessage, string? errorCode, int? statusCode)> CreateTourDateAsync(int visitorId, TourDate tourDate)
    {
        var check = await CheckTourOwnership(visitorId, tourDate.TourId);
        if (check.errorMessage != null) return (null, check.errorMessage, check.errorCode, check.statusCode);

        _tourDateService.Add(tourDate);
        await _unitOfWork.SaveChangesAsync();
        return (tourDate, null, null, null);
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdateTourDateAsync(int visitorId, int id, TourDate tourDate)
    {
        var existing = await _tourDateService.GetByIdAsync(id);
        if (existing == null) return (false, "Tarih bulunamadi", null, 404);

        var check = await CheckTourOwnership(visitorId, existing.TourId);
        if (check.errorMessage != null) return (false, check.errorMessage, check.errorCode, check.statusCode);

        existing.StartDate = tourDate.StartDate;
        existing.EndDate = tourDate.EndDate;
        existing.Price = tourDate.Price;
        existing.MaxCapacity = tourDate.MaxCapacity;
        existing.IsAvailable = tourDate.IsAvailable;
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeleteTourDateAsync(int visitorId, int id)
    {
        var existing = await _tourDateService.GetByIdAsync(id);
        if (existing == null) return (false, true, null, null, null);

        var check = await CheckTourOwnership(visitorId, existing.TourId);
        if (check.errorMessage != null) return (false, false, check.errorMessage, check.errorCode, check.statusCode);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, false, null, null, null);
    }

    public async Task<IEnumerable<object>> GetCheapestDatesAsync(int tourId, string month)
    {
        // month format: "2026-03"
        if (!DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var monthStart))
            return Array.Empty<object>();

        monthStart = DateTime.SpecifyKind(monthStart, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        return await _tourDateService.GetAvailableDates(tourId)
            .Where(td => td.StartDate >= monthStart && td.StartDate < monthEnd)
            .OrderBy(td => td.Price ?? decimal.MaxValue)
            .ThenBy(td => td.StartDate)
            .Select(td => new
            {
                td.Id, td.TourId, td.StartDate, td.EndDate,
                td.Price, td.MaxCapacity, td.BookedCount, td.IsAvailable
            })
            .ToListAsync();
    }

    private async Task<(string? errorMessage, string? errorCode, int? statusCode)> CheckTourOwnership(int visitorId, int tourId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return ("Kullanici bulunamadi", null, 401);
        if (visitor.Company == null) return ("Firma sahibi degilsiniz", "NOT_COMPANY_OWNER", 403);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return ("Firma durumu uygun degil", "COMPANY_NOT_APPROVED", 403);

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return ("Tur bulunamadi", null, 404);
        if (tour.CompanyId != visitor.Company.Id) return ("Bu tur firmaniza ait degil", "NOT_TOUR_OWNER", 403);

        return (null, null, null);
    }
}
