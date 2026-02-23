namespace ErkanTatilPlani.Core.Entities;

public class MessageTemplate : BaseEntity
{
    public int CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int UsageCount { get; set; }

    public virtual Company Company { get; set; } = null!;
}
