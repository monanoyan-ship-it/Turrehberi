using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Services;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public ReviewsController(AppDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    // ===============================================
    // TUR YORUMLARI
    // ===============================================

    /// <summary>
    /// Tur yorumlarini listele (sadece onaylanmis)
    /// </summary>
    [HttpGet("tours/{tourId}/reviews")]
    public async Task<ActionResult<object>> GetTourReviews(
        int tourId,
        [FromQuery] string? sort = "newest",
        [FromQuery] int? rating = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var tour = await _context.Tours.FindAsync(tourId);
        if (tour == null) return NotFound(new { message = "Tur bulunamadi" });

        var query = _context.TourReviews
            .Include(r => r.Visitor)
            .Include(r => r.Replies.Where(reply => reply.IsActive && reply.StatusId == ReviewStatuses.Ids.Approved))
                .ThenInclude(reply => reply.Visitor)
            .Where(r => r.TourId == tourId && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved);

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
            _ => query.OrderByDescending(r => r.CreatedAt) // newest (default)
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
        var ratingDistribution = await _context.TourReviews
            .Where(r => r.TourId == tourId && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .GroupBy(r => r.OverallRating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
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
        });
    }

    /// <summary>
    /// Yeni yorum ekle
    /// </summary>
    [HttpPost("tours/{tourId}/reviews")]
    public async Task<ActionResult<object>> CreateReview(int tourId, [FromBody] CreateReviewRequest request)
    {
        var tour = await _context.Tours.FindAsync(tourId);
        if (tour == null) return NotFound(new { message = "Tur bulunamadi" });

        var visitor = await _context.Visitors.FindAsync(request.VisitorId);
        if (visitor == null) return BadRequest(new { message = "Gecersiz kullanici" });

        // Ayni kullanici ayni tura birden fazla yorum yapamaz
        var existingReview = await _context.TourReviews
            .FirstOrDefaultAsync(r => r.TourId == tourId && r.VisitorId == request.VisitorId && r.IsActive);
        if (existingReview != null)
            return BadRequest(new { message = "Bu tura zaten yorum yapdiniz" });

        // Dogrulanmis yorum kontrolu (rezervasyon yapti mi)
        var hasReservation = await _context.Reservations
            .AnyAsync(r => r.TourId == tourId && r.VisitorId == request.VisitorId &&
                         r.Status == ReservationStatus.Completed);

        var review = new TourReview
        {
            TourId = tourId,
            VisitorId = request.VisitorId,
            OverallRating = Math.Clamp(request.OverallRating, 1, 5),
            ServiceRating = request.ServiceRating.HasValue ? Math.Clamp(request.ServiceRating.Value, 1, 5) : null,
            ValueRating = request.ValueRating.HasValue ? Math.Clamp(request.ValueRating.Value, 1, 5) : null,
            LocationRating = request.LocationRating.HasValue ? Math.Clamp(request.LocationRating.Value, 1, 5) : null,
            OrganizationRating = request.OrganizationRating.HasValue ? Math.Clamp(request.OrganizationRating.Value, 1, 5) : null,
            GuideRating = request.GuideRating.HasValue ? Math.Clamp(request.GuideRating.Value, 1, 5) : null,
            Title = request.Title ?? string.Empty,
            Pros = request.Pros ?? string.Empty,
            Cons = request.Cons ?? string.Empty,
            Comment = request.Comment ?? string.Empty,
            VisitDate = request.VisitDate,
            TravelTypeId = request.TravelTypeId,
            WouldRecommend = request.WouldRecommend,
            IsVerified = hasReservation,
            StatusId = ReviewStatuses.Ids.Approved, // Otomatik onay (moderasyon sonra eklenebilir)
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        _context.TourReviews.Add(review);
        await _context.SaveChangesAsync();

        // Tour istatistiklerini guncelle
        await UpdateTourReviewStats(tourId);

        return Ok(new { message = "Yorumunuz eklendi", reviewId = review.Id });
    }

    /// <summary>
    /// Yorum guncelle
    /// </summary>
    [HttpPut("reviews/{id}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        // Sadece yorum sahibi guncelleyebilir
        if (review.VisitorId != request.VisitorId)
            return Forbid();

        review.OverallRating = Math.Clamp(request.OverallRating, 1, 5);
        review.ServiceRating = request.ServiceRating.HasValue ? Math.Clamp(request.ServiceRating.Value, 1, 5) : null;
        review.ValueRating = request.ValueRating.HasValue ? Math.Clamp(request.ValueRating.Value, 1, 5) : null;
        review.LocationRating = request.LocationRating.HasValue ? Math.Clamp(request.LocationRating.Value, 1, 5) : null;
        review.OrganizationRating = request.OrganizationRating.HasValue ? Math.Clamp(request.OrganizationRating.Value, 1, 5) : null;
        review.GuideRating = request.GuideRating.HasValue ? Math.Clamp(request.GuideRating.Value, 1, 5) : null;
        review.Title = request.Title ?? string.Empty;
        review.Pros = request.Pros ?? string.Empty;
        review.Cons = request.Cons ?? string.Empty;
        review.Comment = request.Comment ?? string.Empty;
        review.VisitDate = request.VisitDate;
        review.TravelTypeId = request.TravelTypeId;
        review.WouldRecommend = request.WouldRecommend;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return Ok(new { message = "Yorum guncellendi" });
    }

    /// <summary>
    /// Yorum sil
    /// </summary>
    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(int id, [FromQuery] int visitorId)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        // Sadece yorum sahibi silebilir
        if (review.VisitorId != visitorId)
            return Forbid();

        review.IsActive = false;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return Ok(new { message = "Yorum silindi" });
    }

    // ===============================================
    // YARDIMCI OYLAMA
    // ===============================================

    /// <summary>
    /// Yorumu yardimci/yardimci degil olarak oyla
    /// </summary>
    [HttpPost("reviews/{id}/helpful")]
    public async Task<IActionResult> VoteHelpful(int id, [FromBody] VoteHelpfulRequest request)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        // Kendi yorumuna oy veremez
        if (review.VisitorId == request.VisitorId)
            return BadRequest(new { message = "Kendi yorumunuza oy veremezsiniz" });

        var existingVote = await _context.ReviewHelpfuls
            .FirstOrDefaultAsync(h => h.ReviewId == id && h.VisitorId == request.VisitorId);

        if (existingVote != null)
        {
            // Oy degistirme
            if (existingVote.IsHelpful != request.IsHelpful)
            {
                if (existingVote.IsHelpful)
                {
                    review.HelpfulCount--;
                    review.NotHelpfulCount++;
                }
                else
                {
                    review.HelpfulCount++;
                    review.NotHelpfulCount--;
                }
                existingVote.IsHelpful = request.IsHelpful;
                existingVote.VotedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Yeni oy
            var vote = new ReviewHelpful
            {
                ReviewId = id,
                VisitorId = request.VisitorId,
                IsHelpful = request.IsHelpful,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            };
            _context.ReviewHelpfuls.Add(vote);

            if (request.IsHelpful)
                review.HelpfulCount++;
            else
                review.NotHelpfulCount++;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = request.IsHelpful ? "Yardimci olarak isaretlendi" : "Yardimci degil olarak isaretlendi",
            helpfulCount = review.HelpfulCount,
            notHelpfulCount = review.NotHelpfulCount
        });
    }

    // ===============================================
    // YANITLAR
    // ===============================================

    /// <summary>
    /// Yoruma yanit ekle
    /// </summary>
    [HttpPost("reviews/{id}/reply")]
    public async Task<ActionResult<object>> AddReply(int id, [FromBody] AddReplyRequest request)
    {
        var review = await _context.TourReviews
            .Include(r => r.Tour)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        var visitor = await _context.Visitors.FindAsync(request.VisitorId);
        if (visitor == null) return BadRequest(new { message = "Gecersiz kullanici" });

        // Firma sahibi mi kontrol et
        var isFromCompany = visitor.UserTypeId == UserTypes.Ids.CompanyOwner &&
                           visitor.CompanyId == review.Tour.CompanyId;

        var reply = new ReviewReply
        {
            ReviewId = id,
            VisitorId = request.VisitorId,
            ParentReplyId = request.ParentReplyId,
            Comment = request.Comment,
            IsFromCompany = isFromCompany,
            StatusId = ReviewStatuses.Ids.Approved,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        _context.ReviewReplies.Add(reply);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Yanit eklendi",
            reply = new
            {
                reply.Id,
                reply.Comment,
                reply.IsFromCompany,
                reply.CreatedAt,
                Visitor = new
                {
                    visitor.Id,
                    visitor.FirstName,
                    LastInitial = visitor.LastName.Length > 0 ? visitor.LastName[0] + "." : ""
                }
            }
        });
    }

    // ===============================================
    // RESIM YUKLEME
    // ===============================================

    /// <summary>
    /// Yoruma resim yukle
    /// </summary>
    [HttpPost("reviews/{id}/images")]
    public async Task<ActionResult<object>> UploadReviewImage(int id, [FromForm] IFormFile file, [FromForm] int visitorId, [FromForm] string? caption = null)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        // Sadece yorum sahibi yukleyebilir
        if (review.VisitorId != visitorId)
            return Forbid();

        // Maksimum 5 resim
        var imageCount = await _context.ReviewImages.CountAsync(ri => ri.ReviewId == id && ri.IsActive);
        if (imageCount >= 5)
            return BadRequest(new { message = "Bir yoruma en fazla 5 resim yukleyebilirsiniz" });

        // Dosya kontrolu
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi" });

        // Yukleme
        using var stream = file.OpenReadStream();
        var result = await _fileUploadService.UploadImageWithThumbnailAsync(stream, file.FileName, "reviews");

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        // Veritabanina kaydet
        var reviewImage = new ReviewImage
        {
            ReviewId = id,
            ImageUrl = result.Url!,
            ThumbnailUrl = result.ThumbnailUrl ?? result.Url!,
            Caption = caption ?? string.Empty,
            DisplayOrder = imageCount + 1,
            FileSize = result.FileSize,
            Width = result.Width,
            Height = result.Height,
            MimeType = result.MimeType ?? "image/jpeg",
            AltText = caption ?? $"Yorum resmi {imageCount + 1}"
        };

        _context.ReviewImages.Add(reviewImage);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Resim yuklendi",
            image = new
            {
                reviewImage.Id,
                reviewImage.ImageUrl,
                reviewImage.ThumbnailUrl,
                reviewImage.Caption,
                reviewImage.DisplayOrder
            }
        });
    }

    /// <summary>
    /// Yorum resimlerini listele
    /// </summary>
    [HttpGet("reviews/{id}/images")]
    public async Task<ActionResult<object>> GetReviewImages(int id)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        var images = await _context.ReviewImages
            .Where(ri => ri.ReviewId == id && ri.IsActive)
            .OrderBy(ri => ri.DisplayOrder)
            .Select(ri => new
            {
                ri.Id,
                ri.ImageUrl,
                ri.ThumbnailUrl,
                ri.Caption,
                ri.DisplayOrder,
                ri.Width,
                ri.Height
            })
            .ToListAsync();

        return Ok(images);
    }

    /// <summary>
    /// Yorum resmini sil
    /// </summary>
    [HttpDelete("reviews/{reviewId}/images/{imageId}")]
    public async Task<IActionResult> DeleteReviewImage(int reviewId, int imageId, [FromQuery] int visitorId)
    {
        var review = await _context.TourReviews.FindAsync(reviewId);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        // Sadece yorum sahibi silebilir
        if (review.VisitorId != visitorId)
            return Forbid();

        var image = await _context.ReviewImages.FirstOrDefaultAsync(ri => ri.Id == imageId && ri.ReviewId == reviewId);
        if (image == null) return NotFound(new { message = "Resim bulunamadi" });

        // Dosyayi sil
        await _fileUploadService.DeleteFileAsync(image.ImageUrl);
        if (!string.IsNullOrEmpty(image.ThumbnailUrl))
            await _fileUploadService.DeleteFileAsync(image.ThumbnailUrl);

        // Veritabanindan sil (soft delete)
        image.IsActive = false;
        image.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Resim silindi" });
    }

    // ===============================================
    // SIKAYET
    // ===============================================

    /// <summary>
    /// Yorum sikayet et
    /// </summary>
    [HttpPost("reviews/{id}/report")]
    public async Task<IActionResult> ReportReview(int id, [FromBody] ReportRequest request)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        // Kendi yorumunu sikayet edemez
        if (review.VisitorId == request.VisitorId)
            return BadRequest(new { message = "Kendi yorumunuzu sikayet edemezsiniz" });

        // Daha once sikayet etti mi
        var existingReport = await _context.ReviewReports
            .FirstOrDefaultAsync(r => r.ReviewId == id && r.VisitorId == request.VisitorId);
        if (existingReport != null)
            return BadRequest(new { message = "Bu yorumu zaten sikayet ettiniz" });

        var report = new ReviewReport
        {
            ReviewId = id,
            VisitorId = request.VisitorId,
            ReasonId = request.ReasonId,
            Description = request.Description ?? string.Empty,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        _context.ReviewReports.Add(report);
        review.ReportCount++;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Sikayet alindi, incelenecektir" });
    }

    // ===============================================
    // FIRMA SAHIBI YORUMLARI
    // ===============================================

    /// <summary>
    /// Firma sahibinin turlarina yapilan yorumlari listele
    /// </summary>
    [HttpGet("reviews/my")]
    public async Task<ActionResult<object>> GetMyReviews([FromQuery] int companyId, [FromQuery] bool? hasReply = null)
    {
        // Firmanin turlarinin ID'leri
        var companyTourIds = await _context.Tours
            .Where(t => t.CompanyId == companyId)
            .Select(t => t.Id)
            .ToListAsync();

        var query = _context.TourReviews
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .Include(r => r.Replies.Where(reply => reply.IsActive))
            .Where(r => companyTourIds.Contains(r.TourId) && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved);

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
        var allReviews = await _context.TourReviews
            .Include(r => r.Replies)
            .Where(r => companyTourIds.Contains(r.TourId) && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .ToListAsync();

        var stats = new
        {
            total = allReviews.Count,
            withReply = allReviews.Count(r => r.Replies.Any(reply => reply.IsFromCompany)),
            withoutReply = allReviews.Count(r => !r.Replies.Any(reply => reply.IsFromCompany)),
            averageRating = allReviews.Any() ? Math.Round(allReviews.Average(r => r.OverallRating), 1) : 0
        };

        return Ok(new { reviews, stats });
    }

    // ===============================================
    // ADMIN MODERASYON
    // ===============================================

    /// <summary>
    /// Tum yorumlari listele (Admin)
    /// </summary>
    [HttpGet("admin/reviews")]
    public async Task<ActionResult<object>> GetAllReviews(
        [FromQuery] int? statusId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.TourReviews
            .Include(r => r.Visitor)
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Where(r => r.IsActive);

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
        var statusStats = await _context.TourReviews
            .Where(r => r.IsActive)
            .GroupBy(r => r.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
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
        });
    }

    /// <summary>
    /// Yorum onayla (Admin)
    /// </summary>
    [HttpPost("reviews/{id}/approve")]
    public async Task<IActionResult> ApproveReview(int id, [FromBody] ModerateReviewRequest request)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        review.StatusId = ReviewStatuses.Ids.Approved;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedById = request.ModeratedById;
        review.ModerationNote = request.Note ?? string.Empty;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return Ok(new { message = "Yorum onaylandi" });
    }

    /// <summary>
    /// Yorum reddet (Admin)
    /// </summary>
    [HttpPost("reviews/{id}/reject")]
    public async Task<IActionResult> RejectReview(int id, [FromBody] RejectReviewRequest request)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return BadRequest(new { message = "Red sebebi zorunludur" });

        review.StatusId = ReviewStatuses.Ids.Rejected;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedById = request.ModeratedById;
        review.ModerationNote = request.Note ?? string.Empty;
        review.RejectionReason = request.RejectionReason;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await UpdateTourReviewStats(review.TourId);

        return Ok(new { message = "Yorum reddedildi" });
    }

    /// <summary>
    /// Yorumu incelemeye al (Flagged durumuna getir)
    /// </summary>
    [HttpPost("reviews/{id}/flag")]
    public async Task<IActionResult> FlagReview(int id, [FromBody] ModerateReviewRequest request)
    {
        var review = await _context.TourReviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Yorum bulunamadi" });

        review.StatusId = ReviewStatuses.Ids.Flagged;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedById = request.ModeratedById;
        review.ModerationNote = request.Note ?? string.Empty;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Yorum incelemeye alindi" });
    }

    /// <summary>
    /// Sikayet raporlarini listele (Admin)
    /// </summary>
    [HttpGet("admin/reports")]
    public async Task<ActionResult<object>> GetReports([FromQuery] int? statusId = null)
    {
        var query = _context.ReviewReports
            .Include(r => r.Review)
                .ThenInclude(r => r!.Tour)
            .Include(r => r.Visitor)
            .Where(r => r.IsActive);

        if (statusId.HasValue)
        {
            query = query.Where(r => r.StatusId == statusId.Value);
        }

        var reports = await query
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

        return Ok(reports);
    }

    /// <summary>
    /// Sikayet coz (Admin)
    /// </summary>
    [HttpPost("reports/{id}/resolve")]
    public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportRequest request)
    {
        var report = await _context.ReviewReports
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound(new { message = "Sikayet bulunamadi" });

        report.StatusId = request.RemoveReview ? 1 : 2; // 1=Kaldirildi, 2=Reddedildi
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedById = request.ReviewedById;
        report.ReviewNote = request.Note ?? string.Empty;

        // Eger sikayet gecerliyse yorumu da reddet
        if (request.RemoveReview && report.Review != null)
        {
            report.Review.StatusId = ReviewStatuses.Ids.Rejected;
            report.Review.RejectionReason = "Sikayet sonucu kaldirildi: " + (request.Note ?? "");
            report.Review.ModeratedAt = DateTime.UtcNow;
            report.Review.ModeratedById = request.ReviewedById;
            await UpdateTourReviewStats(report.Review.TourId);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = request.RemoveReview ? "Yorum kaldirildi" : "Sikayet reddedildi" });
    }

    // ===============================================
    // YARDIMCI METODLAR
    // ===============================================

    private async Task UpdateTourReviewStats(int tourId)
    {
        var stats = await _context.TourReviews
            .Where(r => r.TourId == tourId && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .GroupBy(r => r.TourId)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r => r.OverallRating)
            })
            .FirstOrDefaultAsync();

        var tour = await _context.Tours.FindAsync(tourId);
        if (tour != null)
        {
            tour.ReviewCount = stats?.Count ?? 0;
            tour.AverageRating = stats != null ? Math.Round((decimal)stats.Average, 1) : 0;
            tour.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

// ===============================================
// REQUEST DTO'LARI
// ===============================================

public class CreateReviewRequest
{
    public int VisitorId { get; set; }
    public int OverallRating { get; set; }
    public int? ServiceRating { get; set; }
    public int? ValueRating { get; set; }
    public int? LocationRating { get; set; }
    public int? OrganizationRating { get; set; }
    public int? GuideRating { get; set; }
    public string? Title { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public string? Comment { get; set; }
    public DateTime? VisitDate { get; set; }
    public int TravelTypeId { get; set; }
    public bool WouldRecommend { get; set; } = true;
}

public class UpdateReviewRequest : CreateReviewRequest { }

public class VoteHelpfulRequest
{
    public int VisitorId { get; set; }
    public bool IsHelpful { get; set; }
}

public class AddReplyRequest
{
    public int VisitorId { get; set; }
    public int? ParentReplyId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ReportRequest
{
    public int VisitorId { get; set; }
    public int ReasonId { get; set; }
    public string? Description { get; set; }
}

public class ModerateReviewRequest
{
    public int? ModeratedById { get; set; }
    public string? Note { get; set; }
}

public class RejectReviewRequest
{
    public int? ModeratedById { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class ResolveReportRequest
{
    public int? ReviewedById { get; set; }
    public bool RemoveReview { get; set; }
    public string? Note { get; set; }
}
