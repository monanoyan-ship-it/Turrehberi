namespace ErkanTatilPlani.Core.Factories.Reviews;

public interface ITourReviewFactory
{
    Task<(object? result, string? errorMessage, int statusCode)> GetTourReviewsAsync(int tourId, string? sort, int? rating, int page, int pageSize);
    Task<(object? result, string? errorMessage, int statusCode)> CreateReviewAsync(int tourId, int visitorId, int overallRating, int? serviceRating, int? valueRating, int? locationRating, int? organizationRating, int? guideRating, string? title, string? pros, string? cons, string? comment, DateTime? visitDate, int travelTypeId, bool wouldRecommend, string ipAddress, string userAgent);
    Task<(object? result, string? errorMessage, int statusCode)> UpdateReviewAsync(int id, int visitorId, int overallRating, int? serviceRating, int? valueRating, int? locationRating, int? organizationRating, int? guideRating, string? title, string? pros, string? cons, string? comment, DateTime? visitDate, int travelTypeId, bool wouldRecommend);
    Task<(object? result, string? errorMessage, int statusCode)> DeleteReviewAsync(int id, int visitorId);
    Task<object> GetMyReviewsAsync(int companyId, bool? hasReply);
}
