namespace ErkanTatilPlani.Core.Entities;

public class Conversation : BaseEntity
{
    public int VisitorId { get; set; }
    public int CompanyId { get; set; }
    public int? TourId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public bool IsClosedByCompany { get; set; } = false;
    public bool IsClosedByVisitor { get; set; } = false;

    public virtual Visitor Visitor { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
    public virtual Tour? Tour { get; set; }
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
