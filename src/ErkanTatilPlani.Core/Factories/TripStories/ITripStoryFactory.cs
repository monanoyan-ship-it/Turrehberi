namespace ErkanTatilPlani.Core.Factories.TripStories;

public interface ITripStoryFactory
{
    Task<object> CreateStoryAsync(int visitorId, string title, string content, int? tourId, int? rating, DateTime? travelDate, string? destination, bool isPublic);
    Task<(bool success, string message)> UpdateStoryAsync(int visitorId, int storyId, string title, string content, int? tourId, int? rating, DateTime? travelDate, string? destination, bool isPublic);
    Task<(bool success, string message)> DeleteStoryAsync(int visitorId, int storyId);
    Task<object?> GetStoryAsync(int storyId, int? currentVisitorId);
    Task<object> GetStoriesByVisitorAsync(int visitorId, int page, int pageSize);
    Task<object> GetFeedAsync(int currentVisitorId, int page, int pageSize);
    Task<object> GetPublicFeedAsync(int page, int pageSize);
    Task<(bool success, string message)> ToggleLikeAsync(int visitorId, int storyId);
    Task<object> AddCommentAsync(int visitorId, int storyId, string content, int? parentCommentId);
    Task<(bool success, string message)> DeleteCommentAsync(int visitorId, int commentId);
    Task<object> GetCommentsAsync(int storyId, int page, int pageSize);
    Task<object> UploadPhotoAsync(int visitorId, int storyId, string photoUrl, string? caption);
    Task<(bool success, string message)> DeletePhotoAsync(int visitorId, int photoId);
}
