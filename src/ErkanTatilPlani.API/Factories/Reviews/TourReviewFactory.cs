using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Reviews;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Reviews;

public class TourReviewFactory : ITourReviewFactory
{
    private readonly ITourEntityService _tourService;
    private readonly IReviewEntityService _reviewService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;

    public TourReviewFactory(
        ITourEntityService tourService,
        IReviewEntityService reviewService,
        IVisitorEntityService visitorService,
        IReservationEntityService reservationService,
        IUnitOfWork unitOfWork)
    {
        _tourService = tourService;
        _reviewService = reviewService;
        _visitorService = visitorService;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> GetTourReviewsAsync(int tourId, string? sort, int? rating, int page, int pageSize)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return (null, "Tur bulunamadi", 404);

        var query = _reviewService.GetApprovedReviews()
            .Include(r => r.Visitor)
            .Include(r => r.Replies.Where(reply => reply.IsActive && reply.StatusId == ReviewStatuses.Ids.Approved))
                .ThenInclude(reply => reply.Visitor)
            .Where(r => r.TourId == tourId);

        // Rating filtresi
        if (rating.HasValue && rating >= 1 && rating <= 5)
        {
            query = query.Where(r => r.OverallRating == rating.Value);
        }

        // Siralama
        query = sort switch
        {
            "oldest" => query.OrderBy(r => r.CreatedAt),
            "highest" => query.OrderByDescending(r => r.OverallRating).ThenByDescending(r => r.CreatedAt),
            "lowest" => query.OrderBy(r => r.OverallRating).ThenByDescending(r => r.CreatedAt),
            "helpful" => query.OrderByDescending(r => r.HelpfulCount).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.TourId,
                r.OverallRating,
                r.ServiceRating,
                r.ValueRating,
                r.LocationRating,
                r.OrganizationRating,
                r.GuideRating,
                r.Title,
                r.Pros,
                r.Cons,
                r.Comment,
                r.VisitDate,
                r.TravelTypeId,
                TravelTypeName = TravelTypes.GetById(r.TravelTypeId)!.SystemName,
                TravelTypeIcon = TravelTypes.GetById(r.TravelTypeId)!.Icon,
                r.WouldRecommend,
                r.IsVerified,
                r.HelpfulCount,
                r.NotHelpfulCount,
                r.CreatedAt,
                Visitor = new
                {
                    r.Visitor.Id,
                    r.Visitor.FirstName,
                    LastInitial = r.Visitor.LastName.Length > 0 ? r.Visitor.LastName[0] + "." : ""
                },
                Replies = r.Replies
                    .OrderBy(reply => reply.CreatedAt)
                    .Select(reply => new
                    {
                        reply.Id,
                        reply.Comment,
                        reply.IsFromCompany,
                        reply.CreatedAt,
                        Visitor = new
                        {
                            reply.Visitor.Id,
                            reply.Visitor.FirstName,
                            LastInitial = reply.Visitor.LastName.Length > 0 ? reply.Visitor.LastName[0] + "." : ""
                        }
                    })
            })
            .ToListAsync();

        // Rating dagilimi
        var ratingDistribution = await _reviewService.GetApprovedReviews()
            .Where(r => r.TourId == tourId)
            .GroupBy(r => r.OverallRating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = new
        {
            reviews,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            },
            summary = new
            {
                averageRating = tour.AverageRating,
                reviewCount = tour.ReviewCount,
                ratingDistribution = Enumerable.Range(1, 5).Select(r => new
                {
                    rating = r,
                    count = ratingDistribution.FirstOrDefault(rd => rd.Rating == r)?.Count ?? 0
                })
            }
        };

        return (result, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> CreateReviewAsync(
        int tourId, int visitorId, int overallRating, int? serviceRating, int? valueRating,
        int? locationRating, int? organizationRating, int? guideRating, string? title,
        string? pros, string? cons, string? comment, DateTime? visitDate, int travelTypeId,
        bool wouldRecommend, string ipAddress, string userAgent)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return (null, "Tur bulunamadi", 404);

        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return (null, "Gecersiz kullanici", 400);

        // Ayni kullanici ayni tura birden fazla yorum yapamaz
        var existingReview = await _reviewService.GetByTourAndVisitorAsync(tourId, visitorId);
        if (existingReview != null)
            return (null, "Bu tura zaten yorum yapdiniz", 400);

        // Dogrulanmis yorum kontrolu (rezervasyon yapti mi)
        var hasReservation = await _reservationService.GetActiveReservations()
            .AnyAsync(r => r.TourId == tourId && r.VisitorId == visitorId &&
                         r.Status == ReservationStatuses.Ids.Completed);

        var review = new TourReview
        {
            TourId = tourId,
            VisitorId = visitorId,
            OverallRating = Math.Clamp(overallRating, 1, 5),
            ServiceRating = serviceRating.HasValue ? Math.Clamp(serviceRating.Value, 1, 5) : null,
            ValueRating = valueRating.HasValue ? Math.Clamp(valueRating.Value, 1, 5) : null,
            LocationRating = locationRating.HasValue ? Math.Clamp(locationRating.Value, 1, 5) : null,
            OrganizationRating = organizationRating.HasValue ? Math.Clamp(organizationRating.Value, 1, 5) : null,
            GuideRating = guideRating.HasValue ? Math.Clamp(guideRating.Value, 1, 5) : null,
            Title = title ?? string.Empty,
            Pros = pros ?? string.Empty,
            Cons = cons ?? string.Empty,
            Comment = comment ?? string.Empty,
            VisitDate = visitDate,
            TravelTypeId = travelTypeId,
            WouldRecommend = wouldRecommend,
            IsVerified = hasReservation,
            StatusId = ReviewStatuses.Ids.Approved,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _reviewService.Add(review);
        await _unitOfWork.SaveChangesAsync();

        // Tour istatistiklerini guncelle
        await UpdateTourReviewStats(tourId);

        return (new { message = "Yorumunuz eklendi", reviewId = review.Id }, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> UpdateReviewAsync(
        int id, int visitorId, int overallRating, int? serviceRating, int? valueRating,
        int? locationRating, int? organizationRating, int? guideRating, string? title,
        string? pros, string? cons, string? comment, DateTime? visitDate, int travelTypeId,
        bool wouldRecommend)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        // Sadece yorum sahibi guncelleyebilir
        if (review.VisitorId != visitorId)
            return (null, "Forbid", 403);

        review.OverallRating = Math.Clamp(overallRating, 1, 5);
        review.ServiceRating = serviceRating.HasValue ? Math.Clamp(serviceRating.Value, 1, 5) : null;
        review.ValueRating = valueRating.HasValue ? Math.Clamp(valueRating.Value, 1, 5) : null;
        review.LocationRating = locationRating.HasValue ? Math.Clamp(locationRating.Value, 1, 5) : null;
        review.OrganizationRating = organizationRating.HasValue ? Math.Clamp(organizationRating.Value, 1, 5) : null;
        review.GuideRating = guideRating.HasValue ? Math.Clamp(guideRating.Value, 1, 5) : null;
        review.Title = title ?? string.Empty;
        review.Pros = pros ?? string.Empty;
        review.Cons = cons ?? string.Empty;
        review.Comment = comment ?? string.Empty;
        review.VisitDate = visitDate;
        review.TravelTypeId = travelTypeId;
        review.WouldRecommend = wouldRecommend;
        review.UpdatedAt = DateTime.UtcNow;

        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return (new { message = "Yorum guncellendi" }, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> DeleteReviewAsync(int id, int visitorId)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        // Sadece yorum sahibi silebilir
        if (review.VisitorId != visitorId)
            return (null, "Forbid", 403);

        review.IsActive = false;
        review.UpdatedAt = DateTime.UtcNow;
        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return (new { message = "Yorum silindi" }, null, 200);
    }

    public async Task<object> GetMyReviewsAsync(int companyId, bool? hasReply)
    {
        // Firmanin turlarinin ID'leri
        var companyTours = await _tourService.GetByCompanyIdAsync(companyId);
        var companyTourIds = companyTours.Select(t => t.Id).ToList();

        var query = _reviewService.GetApprovedReviews()
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .Include(r => r.Replies.Where(reply => reply.IsActive))
            .Where(r => companyTourIds.Contains(r.TourId));

        // Yanit filtresi
        if (hasReply.HasValue)
        {
            if (hasReply.Value)
                query = query.Where(r => r.Replies.Any(reply => reply.IsFromCompany));
            else
                query = query.Where(r => !r.Replies.Any(reply => reply.IsFromCompany));
        }

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.TourId,
                TourName = r.Tour.Name,
                TourDestination = r.Tour.Destination,
                r.OverallRating,
                r.Title,
                r.Comment,
                r.Pros,
                r.Cons,
                r.IsVerified,
                r.WouldRecommend,
                r.CreatedAt,
                VisitorName = r.Visitor.FirstName + " " + (r.Visitor.LastName.Length > 0 ? r.Visitor.LastName[0] + "." : ""),
                HasCompanyReply = r.Replies.Any(reply => reply.IsFromCompany),
                CompanyReply = r.Replies
                    .Where(reply => reply.IsFromCompany)
                    .OrderByDescending(reply => reply.CreatedAt)
                    .Select(reply => new
                    {
                        reply.Id,
                        reply.Comment,
                        reply.CreatedAt
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        // Istatistikler
        var allReviews = await _reviewService.GetApprovedReviews()
            .Include(r => r.Replies)
            .Where(r => companyTourIds.Contains(r.TourId))
            .ToListAsync();

        var stats = new
        {
            total = allReviews.Count,
            withReply = allReviews.Count(r => r.Replies.Any(reply => reply.IsFromCompany)),
            withoutReply = allReviews.Count(r => !r.Replies.Any(reply => reply.IsFromCompany)),
            averageRating = allReviews.Any() ? Math.Round(allReviews.Average(r => r.OverallRating), 1) : 0
        };

        return new { reviews, stats };
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
