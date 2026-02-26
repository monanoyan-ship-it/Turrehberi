using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class TripStoryEntityService : ITripStoryEntityService
{
    private readonly AppDbContext _context;

    public TripStoryEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TripStory> GetAll()
        => _context.TripStories.Where(s => s.IsActive);

    public async Task<TripStory?> GetByIdAsync(int id)
        => await _context.TripStories.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

    public IQueryable<TripStory> GetByVisitorId(int visitorId)
        => _context.TripStories.Where(s => s.VisitorId == visitorId && s.IsActive);

    public void Add(TripStory story) => _context.TripStories.Add(story);

    public void Update(TripStory story)
    {
        story.UpdatedAt = DateTime.UtcNow;
        _context.TripStories.Update(story);
    }

    // Likes
    public async Task<TripStoryLike?> GetLikeAsync(int storyId, int visitorId)
        => await _context.TripStoryLikes.FirstOrDefaultAsync(l =>
            l.TripStoryId == storyId && l.VisitorId == visitorId && l.IsActive);

    public void AddLike(TripStoryLike like) => _context.TripStoryLikes.Add(like);

    public void RemoveLike(TripStoryLike like)
    {
        like.IsActive = false;
        like.UpdatedAt = DateTime.UtcNow;
    }

    // Comments
    public IQueryable<TripStoryComment> GetCommentsByStoryId(int storyId)
        => _context.TripStoryComments.Where(c => c.TripStoryId == storyId && c.IsActive);

    public async Task<TripStoryComment?> GetCommentByIdAsync(int commentId)
        => await _context.TripStoryComments.FirstOrDefaultAsync(c => c.Id == commentId && c.IsActive);

    public void AddComment(TripStoryComment comment) => _context.TripStoryComments.Add(comment);
}
