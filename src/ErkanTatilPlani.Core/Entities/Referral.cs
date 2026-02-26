using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class Referral : BaseEntity
{
    public int ReferrerVisitorId { get; set; }
    public int ReferredVisitorId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public int StatusId { get; set; } = ReferralStatuses.Ids.Pending;
    public decimal BonusAmount { get; set; }
    public int? ReservationId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RewardedAt { get; set; }

    public virtual Visitor ReferrerVisitor { get; set; } = null!;
    public virtual Visitor ReferredVisitor { get; set; } = null!;
    public virtual Reservation? Reservation { get; set; }
}
