namespace ErkanTatilPlani.Core.Entities;

public class TripStory : BaseEntity
{
    public int VisitorId { get; set; }
    public int? ReservationId { get; set; }
    public int? TourId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public DateTime? TravelDate { get; set; }
    public string? Destination { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsPublic { get; set; } = true;

    public virtual Visitor Visitor { get; set; } = null!;
    public virtual Reservation? Reservation { get; set; }
    public virtual Tour? Tour { get; set; }
    public virtual ICollection<TripStoryPhoto> Photos { get; set; } = new List<TripStoryPhoto>();
    public virtual ICollection<TripStoryLike> Likes { get; set; } = new List<TripStoryLike>();
    public virtual ICollection<TripStoryComment> Comments { get; set; } = new List<TripStoryComment>();
}
