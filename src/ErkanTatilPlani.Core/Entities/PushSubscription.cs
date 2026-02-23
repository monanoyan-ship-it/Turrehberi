namespace ErkanTatilPlani.Core.Entities;

public class PushSubscription : BaseEntity
{
    public int VisitorId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public virtual Visitor Visitor { get; set; } = null!;
}
