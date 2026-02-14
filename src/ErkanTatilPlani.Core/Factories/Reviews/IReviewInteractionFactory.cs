namespace ErkanTatilPlani.Core.Factories.Reviews;

public interface IReviewInteractionFactory
{
    Task<(object? result, string? errorMessage, int statusCode)> VoteHelpfulAsync(int reviewId, int visitorId, bool isHelpful, string ipAddress);
    Task<(object? result, string? errorMessage, int statusCode)> AddReplyAsync(int reviewId, int visitorId, int? parentReplyId, string comment, string ipAddress);
    Task<(object? result, string? errorMessage, int statusCode)> UploadReviewImageAsync(int reviewId, int visitorId, Stream fileStream, string fileName, string? caption);
    Task<(object? result, string? errorMessage, int statusCode)> GetReviewImagesAsync(int reviewId);
    Task<(object? result, string? errorMessage, int statusCode)> DeleteReviewImageAsync(int reviewId, int imageId, int visitorId);
    Task<(object? result, string? errorMessage, int statusCode)> ReportReviewAsync(int reviewId, int visitorId, int reasonId, string? description, string ipAddress);
}
