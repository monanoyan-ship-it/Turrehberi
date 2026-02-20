namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Rehberin tur tarihine atanmasi
/// </summary>
public class TourGuideAssignment : BaseEntity
{
    public int GuideId { get; set; }
    public virtual Guide Guide { get; set; } = null!;

    public int TourDateId { get; set; }
    public virtual TourDate TourDate { get; set; } = null!;

    /// <summary>
    /// Atama durumu - GuideAssignmentStatuses.Ids uzerinden erisim
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Ek notlar
    /// </summary>
    public string? Notes { get; set; }
}
