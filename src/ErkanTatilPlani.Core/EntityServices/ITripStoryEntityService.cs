using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ITripStoryEntityService
{
    IQueryable<TripStory> GetAll();
    Task<TripStory?> GetByIdAsync(int id);
    IQueryable<TripStory> GetByVisitorId(int visitorId);
    void Add(TripStory story);
    void Update(TripStory story);

    // Likes
    Task<TripStoryLike?> GetLikeAsync(int storyId, int visitorId);
    void AddLike(TripStoryLike like);
    void RemoveLike(TripStoryLike like);

    // Comments
    IQueryable<TripStoryComment> GetCommentsByStoryId(int storyId);
    Task<TripStoryComment?> GetCommentByIdAsync(int commentId);
    void AddComment(TripStoryComment comment);
}
