using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class Notification : BaseEntity
{
    public int VisitorId { get; set; }
    public string TitleKey { get; set; } = string.Empty;
    public string MessageKey { get; set; } = string.Empty;
    public string? MessageParams { get; set; }
    public int NotificationTypeId { get; set; } = NotificationTypes.Ids.System;
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public virtual Visitor Visitor { get; set; } = null!;
}
