namespace ErkanTatilPlani.Core.Entities;

public class TripStoryComment : BaseEntity
{
    public int TripStoryId { get; set; }
    public int VisitorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }

    public virtual TripStory TripStory { get; set; } = null!;
    public virtual Visitor Visitor { get; set; } = null!;
    public virtual TripStoryComment? ParentComment { get; set; }
    public virtual ICollection<TripStoryComment> Replies { get; set; } = new List<TripStoryComment>();
}
