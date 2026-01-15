namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Yorum yardimci oylamalari - Kullanicilar yorumu begendi/begenmedi
/// </summary>
public class ReviewHelpful : BaseEntity
{
    /// <summary>
    /// Oylanan yorum
    /// </summary>
    public int ReviewId { get; set; }
    public virtual TourReview Review { get; set; } = null!;

    /// <summary>
    /// Oylayan kullanici
    /// </summary>
    public int VisitorId { get; set; }
    public virtual Visitor Visitor { get; set; } = null!;

    /// <summary>
    /// Yardimci bulundu mu? (true: helpful, false: not helpful)
    /// </summary>
    public bool IsHelpful { get; set; }

    /// <summary>
    /// Oylama tarihi
    /// </summary>
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP adresi (spam onleme)
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}
