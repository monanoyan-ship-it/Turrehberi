using System.Text.Json;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Notifications;
using ErkanTatilPlani.Core.Factories.Travelers;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Travelers;

public class TravelerProfileFactory : ITravelerProfileFactory
{
    private readonly IVisitorEntityService _visitorService;
    private readonly ITravelerFollowEntityService _followService;
    private readonly ITripStoryEntityService _storyService;
    private readonly IReservationEntityService _reservationService;
    private readonly IReviewEntityService _reviewService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IUnitOfWork _unitOfWork;

    public TravelerProfileFactory(
        IVisitorEntityService visitorService,
        ITravelerFollowEntityService followService,
        ITripStoryEntityService storyService,
        IReservationEntityService reservationService,
        IReviewEntityService reviewService,
        INotificationFactory notificationFactory,
        IUnitOfWork unitOfWork)
    {
        _visitorService = visitorService;
        _followService = followService;
        _storyService = storyService;
        _reservationService = reservationService;
        _reviewService = reviewService;
        _notificationFactory = notificationFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetPublicProfileAsync(int visitorId, int? currentVisitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null)
            return new { error = "Error.VisitorNotFound" };

        if (!visitor.IsProfilePublic && currentVisitorId != visitorId)
            return new { error = "Error.ProfilePrivate", isPrivate = true };

        var tier = LoyaltyTiers.GetById(visitor.LoyaltyTierId);
        var completedTours = await _reservationService.GetByVisitorId(visitorId)
            .CountAsync(r => r.Status == ReservationStatuses.Ids.Completed);
        var reviewCount = await _reviewService.GetActiveReviews().CountAsync(r => r.VisitorId == visitorId);

        bool? isFollowing = null;
        if (currentVisitorId.HasValue && currentVisitorId.Value != visitorId)
        {
            var follow = await _followService.GetByPairAsync(currentVisitorId.Value, visitorId);
            isFollowing = follow != null;
        }

        var recentStories = await _storyService.GetByVisitorId(visitorId)
            .Where(s => s.IsPublic)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new
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
                photoUrl = s.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Select(p => p.PhotoUrl).FirstOrDefault()
            })
            .ToListAsync();

        return new
        {
            id = visitor.Id,
            firstName = visitor.FirstName,
            lastName = visitor.LastName,
            avatarUrl = visitor.AvatarUrl,
            bio = visitor.Bio,
            isProfilePublic = visitor.IsProfilePublic,
            tierNameKey = tier?.NameResourceKey,
            tierIcon = tier?.Icon,
            tierCssClass = tier?.CssClass,
            followerCount = visitor.FollowerCount,
            followingCount = visitor.FollowingCount,
            tripStoryCount = visitor.TripStoryCount,
            completedTours,
            reviewCount,
            isFollowing,
            recentStories
        };
    }

    public async Task<(bool success, string message)> UpdateBioAsync(int visitorId, string bio)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return (false, "Error.VisitorNotFound");

        visitor.Bio = bio?.Trim();
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.BioUpdated");
    }

    public async Task<(bool success, string message)> ToggleProfileVisibilityAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return (false, "Error.VisitorNotFound");

        visitor.IsProfilePublic = !visitor.IsProfilePublic;
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);
        await _unitOfWork.SaveChangesAsync();
        return (true, visitor.IsProfilePublic ? "Success.ProfilePublic" : "Success.ProfilePrivate");
    }

    public async Task<object> GetFollowersAsync(int visitorId, int page, int pageSize)
    {
        var query = _followService.GetByFollowedId(visitorId)
            .Include(f => f.Follower)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                id = f.Follower.Id,
                firstName = f.Follower.FirstName,
                lastName = f.Follower.LastName,
                avatarUrl = f.Follower.AvatarUrl,
                tierNameKey = LoyaltyTiers.GetById(f.Follower.LoyaltyTierId)!.NameResourceKey,
                tierIcon = LoyaltyTiers.GetById(f.Follower.LoyaltyTierId)!.Icon,
                tierCssClass = LoyaltyTiers.GetById(f.Follower.LoyaltyTierId)!.CssClass,
                followedAt = f.CreatedAt
            })
            .ToListAsync();

        return new { items, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<object> GetFollowingAsync(int visitorId, int page, int pageSize)
    {
        var query = _followService.GetByFollowerId(visitorId)
            .Include(f => f.Followed)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                id = f.Followed.Id,
                firstName = f.Followed.FirstName,
                lastName = f.Followed.LastName,
                avatarUrl = f.Followed.AvatarUrl,
                tierNameKey = LoyaltyTiers.GetById(f.Followed.LoyaltyTierId)!.NameResourceKey,
                tierIcon = LoyaltyTiers.GetById(f.Followed.LoyaltyTierId)!.Icon,
                tierCssClass = LoyaltyTiers.GetById(f.Followed.LoyaltyTierId)!.CssClass,
                followedAt = f.CreatedAt
            })
            .ToListAsync();

        return new { items, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<(bool success, string message)> FollowAsync(int currentVisitorId, int targetVisitorId)
    {
        if (currentVisitorId == targetVisitorId)
            return (false, "Error.CannotFollowSelf");

        var target = await _visitorService.GetByIdAsync(targetVisitorId);
        if (target == null) return (false, "Error.VisitorNotFound");

        var existing = await _followService.GetByPairAsync(currentVisitorId, targetVisitorId);
        if (existing != null)
            return (false, "Error.AlreadyFollowing");

        _followService.Add(new TravelerFollow
        {
            FollowerId = currentVisitorId,
            FollowedId = targetVisitorId
        });

        target.FollowerCount++;
        target.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(target);

        var follower = await _visitorService.GetByIdAsync(currentVisitorId);
        if (follower != null)
        {
            follower.FollowingCount++;
            follower.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(follower);
        }

        await _unitOfWork.SaveChangesAsync();

        // Bildirim gonder
        await _notificationFactory.CreateSocialNotificationAsync(
            targetVisitorId, currentVisitorId, NotificationTypes.Ids.NewFollower,
            "Notification.NewFollower.Title", "Notification.NewFollower.Message",
            "Visitor", currentVisitorId);

        return (true, "Success.Followed");
    }

    public async Task<(bool success, string message)> UnfollowAsync(int currentVisitorId, int targetVisitorId)
    {
        var existing = await _followService.GetByPairAsync(currentVisitorId, targetVisitorId);
        if (existing == null)
            return (false, "Error.NotFollowing");

        _followService.Remove(existing);

        var target = await _visitorService.GetByIdAsync(targetVisitorId);
        if (target != null)
        {
            target.FollowerCount = Math.Max(0, target.FollowerCount - 1);
            target.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(target);
        }

        var follower = await _visitorService.GetByIdAsync(currentVisitorId);
        if (follower != null)
        {
            follower.FollowingCount = Math.Max(0, follower.FollowingCount - 1);
            follower.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(follower);
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.Unfollowed");
    }

    public async Task<object> GetTopTravelersAsync(int limit)
    {
        var travelers = await _visitorService.GetAll()
            .Where(v => v.IsProfilePublic && v.IsActive)
            .OrderByDescending(v => v.TripStoryCount)
            .ThenByDescending(v => v.FollowerCount)
            .Take(limit)
            .Select(v => new
            {
                id = v.Id,
                firstName = v.FirstName,
                lastName = v.LastName,
                avatarUrl = v.AvatarUrl,
                bio = v.Bio,
                tierNameKey = LoyaltyTiers.GetById(v.LoyaltyTierId)!.NameResourceKey,
                tierIcon = LoyaltyTiers.GetById(v.LoyaltyTierId)!.Icon,
                tierCssClass = LoyaltyTiers.GetById(v.LoyaltyTierId)!.CssClass,
                followerCount = v.FollowerCount,
                tripStoryCount = v.TripStoryCount
            })
            .ToListAsync();

        return travelers;
    }
}
