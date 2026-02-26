namespace ErkanTatilPlani.Core.Entities;

public class TripStoryLike : BaseEntity
{
    public int TripStoryId { get; set; }
    public int VisitorId { get; set; }

    public virtual TripStory TripStory { get; set; } = null!;
    public virtual Visitor Visitor { get; set; } = null!;
}
