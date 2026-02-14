namespace ErkanTatilPlani.Core.Factories.Reviews;

public interface IAdminReviewFactory
{
    Task<object> GetAllReviewsAsync(int? statusId, string? search, int page, int pageSize);
    Task<(object? result, string? errorMessage, int statusCode)> ApproveReviewAsync(int id, int? moderatedById, string? note);
    Task<(object? result, string? errorMessage, int statusCode)> RejectReviewAsync(int id, int? moderatedById, string rejectionReason, string? note);
    Task<(object? result, string? errorMessage, int statusCode)> FlagReviewAsync(int id, int? moderatedById, string? note);
    Task<object> GetReportsAsync(int? statusId);
    Task<(object? result, string? errorMessage, int statusCode)> ResolveReportAsync(int id, int? reviewedById, bool removeReview, string? note);
}
