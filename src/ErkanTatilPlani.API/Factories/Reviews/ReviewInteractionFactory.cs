using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Reviews;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Reviews;

public class ReviewInteractionFactory : IReviewInteractionFactory
{
    private readonly IReviewEntityService _reviewService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ITourEntityService _tourService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewInteractionFactory(
        IReviewEntityService reviewService,
        IVisitorEntityService visitorService,
        ITourEntityService tourService,
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork)
    {
        _reviewService = reviewService;
        _visitorService = visitorService;
        _tourService = tourService;
        _fileUploadService = fileUploadService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> VoteHelpfulAsync(int reviewId, int visitorId, bool isHelpful, string ipAddress)
    {
        var review = await _reviewService.GetByIdAsync(reviewId);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        // Kendi yorumuna oy veremez
        if (review.VisitorId == visitorId)
            return (null, "Kendi yorumunuza oy veremezsiniz", 400);

        var existingVote = await _reviewService.GetHelpfulVoteAsync(reviewId, visitorId);

        if (existingVote != null)
        {
            // Oy degistirme
            if (existingVote.IsHelpful != isHelpful)
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
                existingVote.IsHelpful = isHelpful;
                existingVote.VotedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Yeni oy
            var vote = new ReviewHelpful
            {
                ReviewId = reviewId,
                VisitorId = visitorId,
                IsHelpful = isHelpful,
                IpAddress = ipAddress
            };
            _reviewService.AddHelpful(vote);

            if (isHelpful)
                review.HelpfulCount++;
            else
                review.NotHelpfulCount++;
        }

        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();

        var result = new
        {
            message = isHelpful ? "Yardimci olarak isaretlendi" : "Yardimci degil olarak isaretlendi",
            helpfulCount = review.HelpfulCount,
            notHelpfulCount = review.NotHelpfulCount
        };

        return (result, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> AddReplyAsync(int reviewId, int visitorId, int? parentReplyId, string comment, string ipAddress)
    {
        // Review with Tour needed to check company ownership
        var review = await _reviewService.GetActiveReviews()
            .Include(r => r.Tour)
            .FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return (null, "Gecersiz kullanici", 400);

        // Firma sahibi mi kontrol et
        var isFromCompany = visitor.UserTypeId == UserTypes.Ids.CompanyOwner &&
                           visitor.CompanyId == review.Tour.CompanyId;

        var reply = new ReviewReply
        {
            ReviewId = reviewId,
            VisitorId = visitorId,
            ParentReplyId = parentReplyId,
            Comment = comment,
            IsFromCompany = isFromCompany,
            StatusId = ReviewStatuses.Ids.Approved,
            IpAddress = ipAddress
        };

        _reviewService.AddReply(reply);
        await _unitOfWork.SaveChangesAsync();

        var result = new
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
        };

        return (result, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> UploadReviewImageAsync(int reviewId, int visitorId, Stream fileStream, string fileName, string? caption)
    {
        var review = await _reviewService.GetByIdAsync(reviewId);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        // Sadece yorum sahibi yukleyebilir
        if (review.VisitorId != visitorId)
            return (null, "Forbid", 403);

        // Maksimum 5 resim
        var imageCount = await _reviewService.GetImageCountAsync(reviewId);
        if (imageCount >= 5)
            return (null, "Bir yoruma en fazla 5 resim yukleyebilirsiniz", 400);

        // Yukleme
        var uploadResult = await _fileUploadService.UploadImageWithThumbnailAsync(fileStream, fileName, "reviews");

        if (!uploadResult.Success)
            return (null, uploadResult.ErrorMessage, 400);

        // Veritabanina kaydet
        var reviewImage = new ReviewImage
        {
            ReviewId = reviewId,
            ImageUrl = uploadResult.Url!,
            ThumbnailUrl = uploadResult.ThumbnailUrl ?? uploadResult.Url!,
            Caption = caption ?? string.Empty,
            DisplayOrder = imageCount + 1,
            FileSize = uploadResult.FileSize,
            Width = uploadResult.Width,
            Height = uploadResult.Height,
            MimeType = uploadResult.MimeType ?? "image/jpeg",
            AltText = caption ?? $"Yorum resmi {imageCount + 1}"
        };

        _reviewService.AddImage(reviewImage);
        await _unitOfWork.SaveChangesAsync();

        var result = new
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
        };

        return (result, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> GetReviewImagesAsync(int reviewId)
    {
        var review = await _reviewService.GetByIdAsync(reviewId);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        var images = await _reviewService.GetActiveImages(reviewId)
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

        return (images, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> DeleteReviewImageAsync(int reviewId, int imageId, int visitorId)
    {
        var review = await _reviewService.GetByIdAsync(reviewId);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        // Sadece yorum sahibi silebilir
        if (review.VisitorId != visitorId)
            return (null, "Forbid", 403);

        var image = await _reviewService.GetImageByIdAsync(imageId, reviewId);
        if (image == null) return (null, "Resim bulunamadi", 404);

        // Dosyayi sil
        await _fileUploadService.DeleteFileAsync(image.ImageUrl);
        if (!string.IsNullOrEmpty(image.ThumbnailUrl))
            await _fileUploadService.DeleteFileAsync(image.ThumbnailUrl);

        // Veritabanindan sil (soft delete)
        image.IsActive = false;
        image.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (new { message = "Resim silindi" }, null, 200);
    }

    public async Task<(object? result, string? errorMessage, int statusCode)> ReportReviewAsync(int reviewId, int visitorId, int reasonId, string? description, string ipAddress)
    {
        var review = await _reviewService.GetByIdAsync(reviewId);
        if (review == null) return (null, "Yorum bulunamadi", 404);

        // Kendi yorumunu sikayet edemez
        if (review.VisitorId == visitorId)
            return (null, "Kendi yorumunuzu sikayet edemezsiniz", 400);

        // Daha once sikayet etti mi
        var existingReport = await _reviewService.GetReportAsync(reviewId, visitorId);
        if (existingReport != null)
            return (null, "Bu yorumu zaten sikayet ettiniz", 400);

        var report = new ReviewReport
        {
            ReviewId = reviewId,
            VisitorId = visitorId,
            ReasonId = reasonId,
            Description = description ?? string.Empty,
            IpAddress = ipAddress
        };

        _reviewService.AddReport(report);
        review.ReportCount++;
        _reviewService.Update(review);
        await _unitOfWork.SaveChangesAsync();

        return (new { message = "Sikayet alindi, incelenecektir" }, null, 200);
    }
}
