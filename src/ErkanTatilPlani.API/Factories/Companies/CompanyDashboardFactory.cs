using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Companies;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Companies;

public class CompanyDashboardFactory : ICompanyDashboardFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly ITourEntityService _tourService;
    private readonly IReservationEntityService _reservationService;
    private readonly IReviewEntityService _reviewService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public CompanyDashboardFactory(
        ICompanyEntityService companyService,
        ITourEntityService tourService,
        IReservationEntityService reservationService,
        IReviewEntityService reviewService,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment env)
    {
        _companyService = companyService;
        _tourService = tourService;
        _reservationService = reservationService;
        _reviewService = reviewService;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    public async Task<(bool found, object? result)> GetCompanyDashboardAsync(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, null);

        // Firmanin turlari
        var companyTours = await _tourService.GetActiveTours()
            .Where(t => t.CompanyId == id)
            .ToListAsync();

        var tourIds = companyTours.Select(t => t.Id).ToList();

        // Istatistikler
        var totalTours = companyTours.Count;
        var activeTours = companyTours.Count(t => t.IsActive);

        // Rezervasyon istatistikleri
        var reservations = await _reservationService.GetByTourIds(tourIds)
            .Where(r => r.IsActive)
            .ToListAsync();

        var totalReservations = reservations.Count;
        var pendingReservations = reservations.Count(r => r.Status == ReservationStatuses.Ids.Pending);
        var confirmedReservations = reservations.Count(r => r.Status == ReservationStatuses.Ids.Confirmed);
        var completedReservations = reservations.Count(r => r.Status == ReservationStatuses.Ids.Completed);

        // Gelir hesaplamasi (onaylanmis ve tamamlanmis rezervasyonlar)
        var totalRevenue = reservations
            .Where(r => r.Status == ReservationStatuses.Ids.Confirmed || r.Status == ReservationStatuses.Ids.Completed)
            .Sum(r => r.TotalPrice);

        // Bu ayin geliri
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthlyRevenue = reservations
            .Where(r => (r.Status == ReservationStatuses.Ids.Confirmed || r.Status == ReservationStatuses.Ids.Completed)
                && r.CreatedAt >= startOfMonth)
            .Sum(r => r.TotalPrice);

        // Yorum istatistikleri
        var reviews = await _reviewService.GetApprovedReviews()
            .Where(r => tourIds.Contains(r.TourId))
            .ToListAsync();

        var totalReviews = reviews.Count;
        var averageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.OverallRating), 1) : 0;

        // Son 5 rezervasyon
        var recentReservations = await _reservationService.GetByTourIds(tourIds)
            .Where(r => r.IsActive)
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                CustomerName = r.Visitor.FirstName + " " + r.Visitor.LastName,
                r.StartDate,
                r.NumberOfPeople,
                r.TotalPrice,
                Status = r.Status == ReservationStatuses.Ids.Pending ? "Pending" : r.Status == ReservationStatuses.Ids.Confirmed ? "Confirmed" : r.Status == ReservationStatuses.Ids.Cancelled ? "Cancelled" : r.Status == ReservationStatuses.Ids.Completed ? "Completed" : "Unknown",
                r.CreatedAt
            })
            .ToListAsync();

        // Son 5 yorum
        var recentReviews = await _reviewService.GetApprovedReviews()
            .Where(r => tourIds.Contains(r.TourId))
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                VisitorName = r.Visitor.FirstName + " " + (r.Visitor.LastName.Length > 0 ? r.Visitor.LastName[0] + "." : ""),
                r.OverallRating,
                r.Title,
                Comment = r.Comment.Length > 100 ? r.Comment.Substring(0, 100) + "..." : r.Comment,
                r.CreatedAt
            })
            .ToListAsync();

        // Tur performansi
        var tourPerformance = new List<object>();
        foreach (var tour in companyTours)
        {
            var tourReservations = reservations.Where(r => r.TourId == tour.Id).ToList();
            var tourReviews = reviews.Where(r => r.TourId == tour.Id).ToList();

            tourPerformance.Add(new
            {
                tour.Id,
                tour.Name,
                ReservationCount = tourReservations.Count,
                Revenue = tourReservations
                    .Where(r => r.Status == ReservationStatuses.Ids.Confirmed || r.Status == ReservationStatuses.Ids.Completed)
                    .Sum(r => r.TotalPrice),
                AverageRating = tourReviews.Any() ? Math.Round(tourReviews.Average(r => r.OverallRating), 1) : 0,
                ReviewCount = tourReviews.Count
            });
        }

        var result = new
        {
            stats = new
            {
                totalTours,
                activeTours,
                totalReservations,
                pendingReservations,
                confirmedReservations,
                completedReservations,
                totalRevenue,
                monthlyRevenue,
                totalReviews,
                averageRating
            },
            recentReservations,
            recentReviews,
            tourPerformance,
            settings = new
            {
                depositPercentage = company.DepositPercentage
            }
        };

        return (true, result);
    }

    public async Task<(bool success, object? result, int statusCode)> UpdateCompanySettingsAsync(int id, int depositPercentage)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        // Validasyon
        if (depositPercentage < 10 || depositPercentage > 100)
            return (false, new { message = "Validation.DepositPercentageRange" }, 400);

        company.DepositPercentage = depositPercentage;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.SettingsUpdated", depositPercentage = company.DepositPercentage }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> UploadContractAsync(int id, Stream fileStream, string fileName, string contentType, long fileLength)
    {
        if (fileStream == null || fileLength == 0)
            return (false, new { message = "Error.NoFileSelected" }, 400);

        // Boyut kontrolu
        if (fileLength > MaxFileSize)
            return (false, new { message = "Error.FileSizeMax10MB" }, 400);

        // Extension kontrolu
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".pdf")
            return (false, new { message = "Error.OnlyPdfFilesAllowed" }, 400);

        // Content-Type kontrolu
        if (contentType != "application/pdf")
            return (false, new { message = "Error.InvalidFileFormat" }, 400);

        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        // Eski dosyayi sil (varsa)
        if (!string.IsNullOrEmpty(company.ContractFileUrl))
        {
            var oldFilePath = Path.Combine(_env.WebRootPath, company.ContractFileUrl.TrimStart('/'));
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        // Dosya adini olustur (guvenli, unique)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var newFileName = $"contract_{id}_{timestamp}.pdf";
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "contracts");

        // Klasor yoksa olustur
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var filePath = Path.Combine(uploadsPath, newFileName);

        // Dosyayi kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Veritabanini guncelle
        company.ContractFileUrl = $"/uploads/contracts/{newFileName}";
        company.ContractUploadedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            message = "Success.ContractUploaded",
            contractFileUrl = company.ContractFileUrl,
            contractUploadedAt = company.ContractUploadedAt
        }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> DeleteContractAsync(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        if (string.IsNullOrEmpty(company.ContractFileUrl))
            return (false, new { message = "Error.NoContractToDelete" }, 400);

        // Dosyayi sil
        var filePath = Path.Combine(_env.WebRootPath, company.ContractFileUrl.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Veritabanini guncelle
        company.ContractFileUrl = string.Empty;
        company.ContractUploadedAt = null;
        company.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.ContractDeleted" }, 200);
    }
}
