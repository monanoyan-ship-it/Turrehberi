namespace ErkanTatilPlani.Core.Entities;

public class SupportTicket : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public int? VisitorId { get; set; }
    public Visitor? Visitor { get; set; }
    public bool IsRead { get; set; }
    public string? AdminReply { get; set; }
    public DateTime? RepliedAt { get; set; }
}
