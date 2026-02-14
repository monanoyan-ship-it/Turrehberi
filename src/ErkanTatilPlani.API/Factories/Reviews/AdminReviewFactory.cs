using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Reviews;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Reviews;

public class AdminReviewFactory : IAdminReviewFactory
{
    private readonly IReviewEntityService _reviewService;
    private readonly ITourEntityService _tourService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminReviewFactory(
        IReviewEntityService reviewService,
        ITourEntityService tourService,
        IUnitOfWork unitOfWork)
    {
        _reviewService = reviewService;
        _tourService = tourService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetAllReviewsAsync(int? statusId, string? search, int page, int pageSize)
    {
        var query = _reviewService.GetActiveReviews()
            .Include(r => r.Visitor)
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .AsQueryable();

        // Durum filtresi
        if (statusId.HasValue)
        {
            query = query.Where(r => r.StatusId == statusId.Value);
        }

        // Arama
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                r.Title.Contains(search) ||
                r.Comment.Contains(search) ||
                r.Visitor.FirstName.Contains(search) ||
                r.Visitor.LastName.Contains(search) ||
                r.Tour.Name.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.TourId,
                TourName = r.Tour.Name,
                CompanyName = r.Tour.Company.Name,
                r.VisitorId,
                VisitorName = r.Visitor.FirstName + " " + r.Visitor.LastName,
                VisitorEmail = r.Visitor.Email,
                r.OverallRating,
                r.Title,
                r.Comment,
                r.Pros,
                r.Cons,
                r.StatusId,
                r.IsVerified,
                r.ReportCount,
                r.HelpfulCount,
                r.NotHelpfulCount,
                r.CreatedAt,
                r.ModeratedAt,
                r.ModerationNote,
                r.RejectionReason,
                r.IpAddress
            })
            .ToListAsync();

        // Durum istatistikleri
        var statusStats = await _reviewService.GetActiveReviews()
            .GroupBy(r => r.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        return new
        {
            reviews,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            },
            stats = new
            {
                pending = statusStats.FirstOrDefault(s => s.StatusId == ReviewStatuses.Ids.Pending)?.Count ?? 0,
                approved = statusStats.FirstOrDefault(s => s.StatusId == ReviewStatuses.Ids.Approved)?.Count ?? 0,
                rejected = statusStats.FirstOrDefault(s => s.StatusId == ReviewStatuses.Ids.Rejected)?.Count ?? 0,
                flagged = statusStats.FirstOrDefault(s => s.StatusId == ReviewStatuses.Ids.Flagged)?.Count ?? 0
            }
        };
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> ApproveReviewAsync(int id, int? moderatedById, string? note)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        review.StatusId = ReviewStatuses.Ids.Approved;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedById = moderatedById;
        review.ModerationNote = note ?? string.Empty;
        review.UpdatedAt = DateTime.UtcNow;

        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return (new { message = "Yorum onaylandi" }, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> RejectReviewAsync(int id, int? moderatedById, string rejectionReason, string? note)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return (null, "Red sebebi zorunludur", 400);

        review.StatusId = ReviewStatuses.Ids.Rejected;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedById = moderatedById;
        review.ModerationNote = note ?? string.Empty;
        review.RejectionReason = rejectionReason;
        review.UpdatedAt = DateTime.UtcNow;

        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return (new { message = "Yorum reddedildi" }, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> FlagReviewAsync(int id, int? moderatedById, string? note)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        review.StatusId = ReviewStatuses.Ids.Flagged;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedById = moderatedById;
        review.ModerationNote = note ?? string.Empty;
        review.UpdatedAt = DateTime.UtcNow;

        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();

        return (new { message = "Yorum incelemeye alindi" }, null, 200);
    }

    public async Task<object> GetReportsAsync(int? statusId)
    {
        var query = _reviewService.GetActiveReports()
            .Include(r => r.Review)
                .ThenInclude(r => r!.Tour)
            .Include(r => r.Visitor);

        IQueryable<Core.Entities.ReviewReport> filteredQuery = query;
        if (statusId.HasValue)
        {
            filteredQuery = filteredQuery.Where(r => r.StatusId == statusId.Value);
        }

        var reports = await filteredQuery
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.ReviewId,
                ReviewTitle = r.Review != null ? r.Review.Title : null,
                ReviewComment = r.Review != null ? r.Review.Comment : null,
                TourName = r.Review != null ? r.Review.Tour.Name : null,
                r.ReasonId,
                r.Description,
                r.StatusId,
                ReporterName = r.Visitor.FirstName + " " + r.Visitor.LastName,
                r.CreatedAt,
                r.ReviewedAt
            })
            .ToListAsync();

        return reports;
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> ResolveReportAsync(int id, int? reviewedById, bool removeReview, string? note)
    {
        var report = await _reviewService.GetActiveReports()
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return (null, "Sikayet bulunamadi", 404);

        report.StatusId = removeReview ? 1 : 2; // 1=Kaldirildi, 2=Reddedildi
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedById = reviewedById;
        report.ReviewNote = note ?? string.Empty;

        // Eger sikayet gecerliyse yorumu da reddet
        if (removeReview && report.Review != null)
        {
            report.Review.StatusId = ReviewStatuses.Ids.Rejected;
            report.Review.RejectionReason = "Sikayet sonucu kaldirildi: " + (note ?? "");
            report.Review.ModeratedAt = DateTime.UtcNow;
            report.Review.ModeratedById = reviewedById;
            _reviewService.Update(report.Review);
            await UpdateTourReviewStats(report.Review.TourId);
        }

        await _unitOfWork.SaveChangesAsync();

        return (new { message = removeReview ? "Yorum kaldirildi" : "Sikayet reddedildi" }, null, 200);
    }

    private async Task UpdateTourReviewStats(int tourId)
    {
        var stats = await _reviewService.GetApprovedReviews()
            .Where(r => r.TourId == tourId)
            .GroupBy(r => r.TourId)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r => r.OverallRating)
            })
            .FirstOrDefaultAsync();

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour != null)
        {
            tour.ReviewCount = stats?.Count ?? 0;
            tour.AverageRating = stats != null ? Math.Round((decimal)stats.Average, 1) : 0;
            tour.UpdatedAt = DateTime.UtcNow;
            _tourService.Update(tour);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
