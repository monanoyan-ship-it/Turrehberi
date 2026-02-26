using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class ScheduledEmail : BaseEntity
{
    public int VisitorId { get; set; }
    public int? ReservationId { get; set; }
    public int EmailTypeId { get; set; }
    public int StatusId { get; set; } = ScheduledEmailStatuses.Ids.Pending;
    public string EmailTo { get; set; } = string.Empty;
    public string? TemplateKey { get; set; }
    public string? TemplateData { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public virtual Visitor Visitor { get; set; } = null!;
    public virtual Reservation? Reservation { get; set; }
}
