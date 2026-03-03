using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Tours;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Tours;

public class TourFactory : ITourFactory
{
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ICompanyEntityService _companyService;
    private readonly ICacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;

    public TourFactory(
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        ICompanyEntityService companyService,
        ICacheService cache,
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService)
    {
        _tourService = tourService;
        _visitorService = visitorService;
        _companyService = companyService;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
    }

    public async Task<object> GetToursAsync(string? search, string? destination, decimal? minPrice, decimal? maxPrice, int? minDays, int? maxDays, int? companyId, bool? featured, string? sort, int? difficulty, int? category, string? guideLanguage, bool? sustainable = null, bool? wheelchairAccessible = null, int? mobilityLevel = null)
    {
        var query = _tourService.GetActiveToursWithCompany()
            .Where(t => t.Company!.StatusId == CompanyStatuses.Ids.Approved);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(searchLower) ||
                                     t.Description.ToLower().Contains(searchLower) ||
                                     t.Destination.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            var destLower = destination.ToLower();
            query = query.Where(t => t.Destination.ToLower().Contains(destLower));
        }

        if (minPrice.HasValue) query = query.Where(t => t.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(t => t.Price <= maxPrice.Value);
        if (minDays.HasValue) query = query.Where(t => t.DurationDays >= minDays.Value);
        if (maxDays.HasValue) query = query.Where(t => t.DurationDays <= maxDays.Value);
        if (companyId.HasValue) query = query.Where(t => t.CompanyId == companyId.Value);
        if (featured.HasValue && featured.Value) query = query.Where(t => t.IsFeatured);
        if (difficulty.HasValue) query = query.Where(t => t.DifficultyId == difficulty.Value);
        if (category.HasValue) query = query.Where(t => t.CategoryId == category.Value);
        if (!string.IsNullOrWhiteSpace(guideLanguage))
        {
            var lang = guideLanguage.ToLower();
            query = query.Where(t => t.GuideLanguages != null && t.GuideLanguages.ToLower().Contains(lang));
        }

        // Faz 15.4 - Surdurulebilirlik filtresi
        if (sustainable.HasValue && sustainable.Value)
            query = query.Where(t => t.IsSustainable);

        // Faz 15.5 - Erisilebilirlik filtreleri
        if (wheelchairAccessible.HasValue && wheelchairAccessible.Value)
            query = query.Where(t => t.IsWheelchairAccessible);
        if (mobilityLevel.HasValue)
            query = query.Where(t => t.MobilityLevel == mobilityLevel.Value);

        query = sort?.ToLower() switch
        {
            "price_asc" => query.OrderBy(t => t.Price),
            "price_desc" => query.OrderByDescending(t => t.Price),
            "duration_asc" => query.OrderBy(t => t.DurationDays),
            "duration_desc" => query.OrderByDescending(t => t.DurationDays),
            "rating" => query.OrderByDescending(t => t.AverageRating),
            "newest" => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.IsFeatured).ThenByDescending(t => t.AverageRating)
        };

        // Aktif rezervasyon sayilarini da getir (kitlik badge icin)
        var toursWithReservations = await query
            .Select(t => new
            {
                Tour = t,
                ActiveReservationCount = t.Reservations.Count(r => r.IsActive && (r.Status == ReservationStatuses.Ids.Pending || r.Status == ReservationStatuses.Ids.Confirmed))
            })
            .ToListAsync();

        var tours = toursWithReservations.Select(t => new
        {
            t.Tour.Id, t.Tour.Name, t.Tour.Description, t.Tour.Destination,
            t.Tour.Price, t.Tour.DurationDays, t.Tour.MaxCapacity,
            t.Tour.ImageUrl, t.Tour.IsFeatured,
            t.Tour.Latitude, t.Tour.Longitude,
            t.Tour.DifficultyId, t.Tour.CategoryId,
            t.Tour.GuideLanguages, t.Tour.Inclusions, t.Tour.Exclusions,
            t.Tour.MeetingPointLat, t.Tour.MeetingPointLng, t.Tour.MeetingPointAddress,
            t.Tour.CompanyId, Company = t.Tour.Company,
            t.Tour.ReviewCount, t.Tour.AverageRating,
            t.ActiveReservationCount,
            t.Tour.IsSustainable, t.Tour.SustainabilityScore,
            t.Tour.IsWheelchairAccessible, t.Tour.MobilityLevel,
            t.Tour.CreatedAt, t.Tour.IsActive
        }).ToList();

        var allTours = await _tourService.GetActiveToursWithCompany()
            .Where(t => t.Company!.StatusId == CompanyStatuses.Ids.Approved)
            .ToListAsync();

        var destinations = allTours.Select(t => t.Destination).Distinct().OrderBy(d => d).ToList();
        var priceRange = new { min = allTours.Any() ? allTours.Min(t => t.Price) : 0, max = allTours.Any() ? allTours.Max(t => t.Price) : 0 };
        var durationRange = new { min = allTours.Any() ? allTours.Min(t => t.DurationDays) : 1, max = allTours.Any() ? allTours.Max(t => t.DurationDays) : 30 };

        return new { tours, filters = new { destinations, priceRange, durationRange }, totalCount = tours.Count };
    }

    public async Task<IEnumerable<object>> GetTourCategoriesAsync()
    {
        return await Task.FromResult(TourCategories.All.Select(c => new
        {
            c.Id, c.SystemName, c.NameResourceKey, c.Icon, c.CssClass
        }).ToList());
    }

    public async Task<IEnumerable<Tour>> GetFeaturedToursAsync()
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.FeaturedTours,
            async () => await _tourService.GetActiveToursWithCompany()
                .Where(t => t.IsFeatured && t.Company!.StatusId == CompanyStatuses.Ids.Approved)
                .ToListAsync(),
            CacheDurations.Medium
        );
    }

    public async Task<object> GetTourCompaniesAsync()
    {
        return await _companyService.GetActiveApprovedCompanies()
            .Where(c => c.Tours.Any(t => t.IsActive))
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
    }

    public async Task<Tour?> GetTourAsync(int id)
        => await _tourService.GetByIdWithCompanyAsync(id);

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetMyToursAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null)
            return (null, "Error.UserNotFound", null, 401);
        if (visitor.Company == null)
            return (null, "Error.NotCompanyOwner", "NOT_COMPANY_OWNER", 403);

        var tours = await _tourService.GetActiveTours()
            .Where(t => t.CompanyId == visitor.Company.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id, t.Name, t.Description, t.Destination, t.Price, t.DurationDays,
                t.MaxCapacity, t.ImageUrl, t.IsFeatured, t.IsActive,
                t.ReviewCount, t.AverageRating, t.CreatedAt,
                ReservationCount = t.Reservations.Count(r => r.IsActive)
            })
            .ToListAsync();

        return (new { tours, companyStatus = visitor.Company.StatusId, canManageTours = visitor.Company.StatusId == CompanyStatuses.Ids.Approved }, null, null, null);
    }

    public async Task<(Tour? tour, string? errorMessage, string? errorCode, int? statusCode)> CreateTourAsync(int visitorId, Tour tour)
    {
        var check = await CheckCompanyStatus(visitorId);
        if (check.errorMessage != null) return (null, check.errorMessage, check.errorCode, check.statusCode);

        _tourService.Add(tour);
        await _unitOfWork.SaveChangesAsync();
        return (tour, null, null, null);
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdateTourAsync(int visitorId, int id, Tour tour)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return (false, "Error.UserNotFound", null, 401);
        if (visitor.Company == null) return (false, "Error.NotCompanyOwner", "NOT_COMPANY_OWNER", 403);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, "Error.CompanyStatusInvalid", "COMPANY_NOT_APPROVED", 403);

        var existing = await _tourService.GetByIdAsync(id);
        if (existing == null) return (false, "Error.TourNotFound", null, 404);
        if (existing.CompanyId != visitor.Company.Id)
            return (false, "Error.TourNotOwnedByCompany", "NOT_TOUR_OWNER", 403);

        // CompanyOwner IsFeatured degistiremez - sadece Admin degistirebilir
        if (visitor.UserTypeId != UserTypes.Ids.Admin)
        {
            tour.IsFeatured = existing.IsFeatured;
        }

        existing.Name = tour.Name;
        existing.Description = tour.Description;
        existing.Destination = tour.Destination;
        existing.Price = tour.Price;
        existing.DurationDays = tour.DurationDays;
        existing.MaxCapacity = tour.MaxCapacity;
        existing.ImageUrl = tour.ImageUrl;
        existing.IsFeatured = tour.IsFeatured;
        existing.DifficultyId = tour.DifficultyId;
        existing.CategoryId = tour.CategoryId;
        existing.GuideLanguages = tour.GuideLanguages;
        existing.Inclusions = tour.Inclusions;
        existing.Exclusions = tour.Exclusions;
        existing.Latitude = tour.Latitude;
        existing.Longitude = tour.Longitude;
        existing.MeetingPointLat = tour.MeetingPointLat;
        existing.MeetingPointLng = tour.MeetingPointLng;
        existing.MeetingPointAddress = tour.MeetingPointAddress;
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeleteTourAsync(int visitorId, int id)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return (false, false, "Error.UserNotFound", null, 401);
        if (visitor.Company == null) return (false, false, "Error.NotCompanyOwner", "NOT_COMPANY_OWNER", 403);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, false, "Error.CompanyStatusInvalid", "COMPANY_NOT_APPROVED", 403);

        var tour = await _tourService.GetByIdAsync(id);
        if (tour == null) return (false, true, null, null, null);

        tour.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return (true, false, null, null, null);
    }

    public async Task<(bool success, object? result, int statusCode)> UploadCoverPhotoAsync(int visitorId, int tourId, Stream fileStream, string fileName)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return (false, new { message = "Error.UserNotFound" }, 401);
        if (visitor.Company == null) return (false, new { message = "Error.NotCompanyOwner" }, 403);

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null || !tour.IsActive) return (false, new { message = "Error.TourNotFound" }, 404);
        if (tour.CompanyId != visitor.Company.Id) return (false, new { message = "Error.TourNotOwnedByCompany" }, 403);

        // Eski fotoğrafı sil
        if (!string.IsNullOrEmpty(tour.ImageUrl) && tour.ImageUrl.StartsWith("/uploads/"))
        {
            await _fileUploadService.DeleteFileAsync(tour.ImageUrl);
        }

        var uploadResult = await _fileUploadService.UploadImageAsync(fileStream, fileName, "tours", 1200);
        if (!uploadResult.Success)
            return (false, new { message = uploadResult.ErrorMessage }, 400);

        tour.ImageUrl = uploadResult.Url ?? string.Empty;
        tour.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { imageUrl = uploadResult.Url }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> DeleteCoverPhotoAsync(int visitorId, int tourId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return (false, new { message = "Error.UserNotFound" }, 401);
        if (visitor.Company == null) return (false, new { message = "Error.NotCompanyOwner" }, 403);

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null || !tour.IsActive) return (false, new { message = "Error.TourNotFound" }, 404);
        if (tour.CompanyId != visitor.Company.Id) return (false, new { message = "Error.TourNotOwnedByCompany" }, 403);

        if (!string.IsNullOrEmpty(tour.ImageUrl) && tour.ImageUrl.StartsWith("/uploads/"))
        {
            await _fileUploadService.DeleteFileAsync(tour.ImageUrl);
        }

        tour.ImageUrl = string.Empty;
        tour.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.CoverPhotoDeleted" }, 200);
    }

    private async Task<(string? errorMessage, string? errorCode, int? statusCode)> CheckCompanyStatus(int visitorId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return ("Error.UserNotFound", null, 401);
        if (visitor.Company == null) return ("Error.NotCompanyOwner", "NOT_COMPANY_OWNER", 403);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
        {
            var msg = visitor.Company.StatusId switch
            {
                0 => ("Error.CompanyPendingApproval", "COMPANY_PENDING"),
                2 => ("Error.CompanyRejected", "COMPANY_REJECTED"),
                3 => ("Error.CompanySuspended", "COMPANY_SUSPENDED"),
                _ => ("Error.CompanyInvalidStatus", "COMPANY_INVALID_STATUS")
            };
            return (msg.Item1, msg.Item2, 403);
        }
        return (null, null, null);
    }
}
