using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class ReviewEntityService : IReviewEntityService
{
    private readonly AppDbContext _context;

    public ReviewEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TourReview> GetActiveReviews()
        => _context.TourReviews.Where(r => r.IsActive);

    public IQueryable<TourReview> GetApprovedReviews()
        => _context.TourReviews.Where(r => r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved);

    public async Task<TourReview?> GetByIdAsync(int id)
        => await _context.TourReviews.FindAsync(id);

    public async Task<TourReview?> GetByTourAndVisitorAsync(int tourId, int visitorId)
        => await _context.TourReviews.FirstOrDefaultAsync(r => r.TourId == tourId && r.VisitorId == visitorId && r.IsActive);

    public void Add(TourReview review) => _context.TourReviews.Add(review);

    public void Update(TourReview review) => _context.Entry(review).State = EntityState.Modified;

    // ReviewHelpful
    public async Task<ReviewHelpful?> GetHelpfulVoteAsync(int reviewId, int visitorId)
        => await _context.ReviewHelpfuls.FirstOrDefaultAsync(h => h.ReviewId == reviewId && h.VisitorId == visitorId);

    public void AddHelpful(ReviewHelpful helpful) => _context.ReviewHelpfuls.Add(helpful);

    // ReviewReply
    public void AddReply(ReviewReply reply) => _context.ReviewReplies.Add(reply);

    // ReviewImage
    public async Task<int> GetImageCountAsync(int reviewId)
        => await _context.ReviewImages.CountAsync(ri => ri.ReviewId == reviewId && ri.IsActive);

    public async Task<ReviewImage?> GetImageByIdAsync(int imageId, int reviewId)
        => await _context.ReviewImages.FirstOrDefaultAsync(ri => ri.Id == imageId && ri.ReviewId == reviewId);

    public void AddImage(ReviewImage image) => _context.ReviewImages.Add(image);

    public IQueryable<ReviewImage> GetActiveImages(int reviewId)
        => _context.ReviewImages.Where(ri => ri.ReviewId == reviewId && ri.IsActive);

    // ReviewReport
    public async Task<ReviewReport?> GetReportAsync(int reviewId, int visitorId)
        => await _context.ReviewReports.FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.VisitorId == visitorId);

    public async Task<ReviewReport?> GetReportByIdAsync(int id)
        => await _context.ReviewReports.Include(r => r.Review).FirstOrDefaultAsync(r => r.Id == id);

    public void AddReport(ReviewReport report) => _context.ReviewReports.Add(report);

    public IQueryable<ReviewReport> GetActiveReports()
        => _context.ReviewReports.Where(r => r.IsActive);
}
