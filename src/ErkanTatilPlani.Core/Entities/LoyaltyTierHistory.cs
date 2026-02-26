using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class LoyaltyTierHistory : BaseEntity
{
    public int VisitorId { get; set; }
    public int PreviousTierId { get; set; } = LoyaltyTiers.Ids.Explorer;
    public int NewTierId { get; set; }
    public int CompletedTourCount { get; set; }
    public string? Reason { get; set; }

    public virtual Visitor Visitor { get; set; } = null!;
}
