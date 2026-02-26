using System.Text.Json;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Notifications;
using ErkanTatilPlani.Core.Factories.TripStories;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.TripStories;

public class TripStoryFactory : ITripStoryFactory
{
    private readonly ITripStoryEntityService _storyService;
    private readonly ITripStoryPhotoEntityService _photoService;
    private readonly ITravelerFollowEntityService _followService;
    private readonly IVisitorEntityService _visitorService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IUnitOfWork _unitOfWork;

    public TripStoryFactory(
        ITripStoryEntityService storyService,
        ITripStoryPhotoEntityService photoService,
        ITravelerFollowEntityService followService,
        IVisitorEntityService visitorService,
        INotificationFactory notificationFactory,
        IUnitOfWork unitOfWork)
    {
        _storyService = storyService;
        _photoService = photoService;
        _followService = followService;
        _visitorService = visitorService;
        _notificationFactory = notificationFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> CreateStoryAsync(int visitorId, string title, string content, int? tourId, int? rating, DateTime? travelDate, string? destination, bool isPublic)
    {
        var story = new TripStory
        {
            VisitorId = visitorId,
            Title = title.Trim(),
            Content = content.Trim(),
            TourId = tourId,
            Rating = rating,
            TravelDate = travelDate,
            Destination = destination?.Trim(),
            IsPublic = isPublic
        };

        _storyService.Add(story);

        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor != null)
        {
            visitor.TripStoryCount++;
            visitor.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(visitor);
        }

        await _unitOfWork.SaveChangesAsync();

        return new { id = story.Id, message = "Success.StoryCreated" };
    }

    public async Task<(bool success, string message)> UpdateStoryAsync(int visitorId, int storyId, string title, string content, int? tourId, int? rating, DateTime? travelDate, string? destination, bool isPublic)
    {
        var story = await _storyService.GetByIdAsync(storyId);
        if (story == null) return (false, "Error.StoryNotFound");
        if (story.VisitorId != visitorId) return (false, "Error.Unauthorized");

        story.Title = title.Trim();
        story.Content = content.Trim();
        story.TourId = tourId;
        story.Rating = rating;
        story.TravelDate = travelDate;
        story.Destination = destination?.Trim();
        story.IsPublic = isPublic;

        _storyService.Update(story);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.StoryUpdated");
    }

    public async Task<(bool success, string message)> DeleteStoryAsync(int visitorId, int storyId)
    {
        var story = await _storyService.GetByIdAsync(storyId);
        if (story == null) return (false, "Error.StoryNotFound");
        if (story.VisitorId != visitorId) return (false, "Error.Unauthorized");

        story.IsActive = false;
        story.UpdatedAt = DateTime.UtcNow;
        _storyService.Update(story);

        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor != null)
        {
            visitor.TripStoryCount = Math.Max(0, visitor.TripStoryCount - 1);
            visitor.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(visitor);
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.StoryDeleted");
    }

    public async Task<object?> GetStoryAsync(int storyId, int? currentVisitorId)
    {
        var story = await _storyService.GetAll()
            .Include(s => s.Visitor)
            .Include(s => s.Tour)
            .Include(s => s.Photos.Where(p => p.IsActive))
            .FirstOrDefaultAsync(s => s.Id == storyId);

        if (story == null) return null;

        if (!story.IsPublic && story.VisitorId != currentVisitorId)
            return new { error = "Error.StoryPrivate" };

        bool? isLiked = null;
        if (currentVisitorId.HasValue)
        {
            var like = await _storyService.GetLikeAsync(storyId, currentVisitorId.Value);
            isLiked = like != null;
        }

        var tier = LoyaltyTiers.GetById(story.Visitor.LoyaltyTierId);

        return new
        {
            story.Id,
            story.Title,
            story.Content,
            story.Rating,
            story.TravelDate,
            story.Destination,
            story.LikeCount,
            story.CommentCount,
            story.IsPublic,
            story.CreatedAt,
            tourId = story.TourId,
            tourName = story.Tour?.Name,
            visitor = new
            {
                id = story.Visitor.Id,
                firstName = story.Visitor.FirstName,
                lastName = story.Visitor.LastName,
                avatarUrl = story.Visitor.AvatarUrl,
                tierNameKey = tier?.NameResourceKey,
                tierIcon = tier?.Icon,
                tierCssClass = tier?.CssClass
            },
            photos = story.Photos.OrderBy(p => p.DisplayOrder).Select(p => new
            {
                p.Id,
                p.PhotoUrl,
                p.Caption,
                p.DisplayOrder
            }),
            isLiked
        };
    }

    public async Task<object> GetStoriesByVisitorAsync(int visitorId, int page, int pageSize)
    {
        var query = _storyService.GetByVisitorId(visitorId)
            .Where(s => s.IsPublic)
            .OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Visitor)
            .Include(s => s.Tour)
            .Select(s => MapStoryCard(s))
            .ToListAsync();

        return new { items, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<object> GetFeedAsync(int currentVisitorId, int page, int pageSize)
    {
        var followingIds = await _followService.GetByFollowerId(currentVisitorId)
            .Select(f => f.FollowedId)
            .ToListAsync();

        var query = _storyService.GetAll()
            .Where(s => s.IsPublic && followingIds.Contains(s.VisitorId))
            .OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Visitor)
            .Include(s => s.Tour)
            .Include(s => s.Photos.Where(p => p.IsActive))
            .Select(s => MapStoryCard(s))
            .ToListAsync();

        return new { items, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<object> GetPublicFeedAsync(int page, int pageSize)
    {
        var query = _storyService.GetAll()
            .Where(s => s.IsPublic)
            .OrderByDescending(s => s.LikeCount)
            .ThenByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Visitor)
            .Include(s => s.Tour)
            .Include(s => s.Photos.Where(p => p.IsActive))
            .Select(s => MapStoryCard(s))
            .ToListAsync();

        return new { items, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<(bool success, string message)> ToggleLikeAsync(int visitorId, int storyId)
    {
        var story = await _storyService.GetByIdAsync(storyId);
        if (story == null) return (false, "Error.StoryNotFound");

        var existingLike = await _storyService.GetLikeAsync(storyId, visitorId);
        if (existingLike != null)
        {
            _storyService.RemoveLike(existingLike);
            story.LikeCount = Math.Max(0, story.LikeCount - 1);
            _storyService.Update(story);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Success.Unliked");
        }

        _storyService.AddLike(new TripStoryLike
        {
            TripStoryId = storyId,
            VisitorId = visitorId
        });
        story.LikeCount++;
        _storyService.Update(story);
        await _unitOfWork.SaveChangesAsync();

        // Bildirim
        if (story.VisitorId != visitorId)
        {
            await _notificationFactory.CreateSocialNotificationAsync(
                story.VisitorId, visitorId, NotificationTypes.Ids.StoryLiked,
                "Notification.StoryLiked.Title", "Notification.StoryLiked.Message",
                "TripStory", storyId);
        }

        return (true, "Success.Liked");
    }

    public async Task<object> AddCommentAsync(int visitorId, int storyId, string content, int? parentCommentId)
    {
        var story = await _storyService.GetByIdAsync(storyId);
        if (story == null) return new { error = "Error.StoryNotFound" };

        var comment = new TripStoryComment
        {
            TripStoryId = storyId,
            VisitorId = visitorId,
            Content = content.Trim(),
            ParentCommentId = parentCommentId
        };

        _storyService.AddComment(comment);
        story.CommentCount++;
        _storyService.Update(story);
        await _unitOfWork.SaveChangesAsync();

        // Bildirim
        if (story.VisitorId != visitorId)
        {
            await _notificationFactory.CreateSocialNotificationAsync(
                story.VisitorId, visitorId, NotificationTypes.Ids.StoryCommented,
                "Notification.StoryCommented.Title", "Notification.StoryCommented.Message",
                "TripStory", storyId);
        }

        return new { id = comment.Id, message = "Success.CommentAdded" };
    }

    public async Task<(bool success, string message)> DeleteCommentAsync(int visitorId, int commentId)
    {
        var comment = await _storyService.GetCommentByIdAsync(commentId);
        if (comment == null) return (false, "Error.CommentNotFound");
        if (comment.VisitorId != visitorId) return (false, "Error.Unauthorized");

        comment.IsActive = false;
        comment.UpdatedAt = DateTime.UtcNow;

        var story = await _storyService.GetByIdAsync(comment.TripStoryId);
        if (story != null)
        {
            story.CommentCount = Math.Max(0, story.CommentCount - 1);
            _storyService.Update(story);
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.CommentDeleted");
    }

    public async Task<object> GetCommentsAsync(int storyId, int page, int pageSize)
    {
        var query = _storyService.GetCommentsByStoryId(storyId)
            .Where(c => c.ParentCommentId == null)
            .Include(c => c.Visitor)
            .Include(c => c.Replies.Where(r => r.IsActive))
                .ThenInclude(r => r.Visitor)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                visitor = new
                {
                    id = c.Visitor.Id,
                    firstName = c.Visitor.FirstName,
                    lastName = c.Visitor.LastName,
                    avatarUrl = c.Visitor.AvatarUrl
                },
                replies = c.Replies.OrderBy(r => r.CreatedAt).Select(r => new
                {
                    r.Id,
                    r.Content,
                    r.CreatedAt,
                    visitor = new
                    {
                        id = r.Visitor.Id,
                        firstName = r.Visitor.FirstName,
                        lastName = r.Visitor.LastName,
                        avatarUrl = r.Visitor.AvatarUrl
                    }
                })
            })
            .ToListAsync();

        return new { items, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<object> UploadPhotoAsync(int visitorId, int storyId, string photoUrl, string? caption)
    {
        var story = await _storyService.GetByIdAsync(storyId);
        if (story == null) return new { error = "Error.StoryNotFound" };
        if (story.VisitorId != visitorId) return new { error = "Error.Unauthorized" };

        var maxOrder = await _photoService.GetByStoryId(storyId).MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

        var photo = new TripStoryPhoto
        {
            TripStoryId = storyId,
            PhotoUrl = photoUrl,
            Caption = caption?.Trim(),
            DisplayOrder = maxOrder + 1
        };

        _photoService.Add(photo);
        await _unitOfWork.SaveChangesAsync();

        return new { id = photo.Id, photoUrl = photo.PhotoUrl, message = "Success.PhotoUploaded" };
    }

    public async Task<(bool success, string message)> DeletePhotoAsync(int visitorId, int photoId)
    {
        var photo = await _photoService.GetByIdAsync(photoId);
        if (photo == null) return (false, "Error.PhotoNotFound");

        var story = await _storyService.GetByIdAsync(photo.TripStoryId);
        if (story == null || story.VisitorId != visitorId) return (false, "Error.Unauthorized");

        _photoService.Remove(photo);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.PhotoDeleted");
    }

    private static object MapStoryCard(TripStory s)
    {
        var tier = LoyaltyTiers.GetById(s.Visitor.LoyaltyTierId);
        return new
        {
            s.Id,
            s.Title,
            contentPreview = s.Content.Length > 200 ? s.Content.Substring(0, 200) + "..." : s.Content,
            s.Rating,
            s.TravelDate,
            s.Destination,
            s.LikeCount,
            s.CommentCount,
            s.CreatedAt,
            tourName = s.Tour != null ? s.Tour.Name : null,
            tourId = s.TourId,
            photoUrl = s.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Select(p => p.PhotoUrl).FirstOrDefault(),
            visitor = new
            {
                id = s.Visitor.Id,
                firstName = s.Visitor.FirstName,
                lastName = s.Visitor.LastName,
                avatarUrl = s.Visitor.AvatarUrl,
                tierNameKey = tier?.NameResourceKey,
                tierIcon = tier?.Icon,
                tierCssClass = tier?.CssClass
            }
        };
    }
}
