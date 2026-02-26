namespace ErkanTatilPlani.Core.Entities;

public class TravelerFollow : BaseEntity
{
    public int FollowerId { get; set; }
    public int FollowedId { get; set; }

    public virtual Visitor Follower { get; set; } = null!;
    public virtual Visitor Followed { get; set; } = null!;
}
