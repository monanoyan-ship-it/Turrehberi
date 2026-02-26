namespace ErkanTatilPlani.Core.Factories.Travelers;

public interface ITravelerProfileFactory
{
    Task<object> GetPublicProfileAsync(int visitorId, int? currentVisitorId);
    Task<(bool success, string message)> UpdateBioAsync(int visitorId, string bio);
    Task<(bool success, string message)> ToggleProfileVisibilityAsync(int visitorId);
    Task<object> GetFollowersAsync(int visitorId, int page, int pageSize);
    Task<object> GetFollowingAsync(int visitorId, int page, int pageSize);
    Task<(bool success, string message)> FollowAsync(int currentVisitorId, int targetVisitorId);
    Task<(bool success, string message)> UnfollowAsync(int currentVisitorId, int targetVisitorId);
    Task<object> GetTopTravelersAsync(int limit);
}
