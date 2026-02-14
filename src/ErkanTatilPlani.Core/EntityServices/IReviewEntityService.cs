using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IReviewEntityService
{
    IQueryable<TourReview> GetActiveReviews();
    IQueryable<TourReview> GetApprovedReviews();
    Task<TourReview?> GetByIdAsync(int id);
    Task<TourReview?> GetByTourAndVisitorAsync(int tourId, int visitorId);
    void Add(TourReview review);
    void Update(TourReview review);
    // ReviewHelpful
    Task<ReviewHelpful?> GetHelpfulVoteAsync(int reviewId, int visitorId);
    void AddHelpful(ReviewHelpful helpful);
    // ReviewReply
    void AddReply(ReviewReply reply);
    // ReviewImage
    Task<int> GetImageCountAsync(int reviewId);
    Task<ReviewImage?> GetImageByIdAsync(int imageId, int reviewId);
    void AddImage(ReviewImage image);
    IQueryable<ReviewImage> GetActiveImages(int reviewId);
    // ReviewReport
    Task<ReviewReport?> GetReportAsync(int reviewId, int visitorId);
    Task<ReviewReport?> GetReportByIdAsync(int id);
    void AddReport(ReviewReport report);
    IQueryable<ReviewReport> GetActiveReports();
}
