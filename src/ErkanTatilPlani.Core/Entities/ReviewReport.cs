namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Yorum sikayetleri - Uygunsuz icerik bildirimi
/// </summary>
public class ReviewReport : BaseEntity
{
    /// <summary>
    /// Sikayet edilen yorum (ReviewReply icin null)
    /// </summary>
    public int? ReviewId { get; set; }
    public virtual TourReview? Review { get; set; }

    /// <summary>
    /// Sikayet edilen yanit (TourReview icin null)
    /// </summary>
    public int? ReplyId { get; set; }
    public virtual ReviewReply? Reply { get; set; }

    /// <summary>
    /// Sikayet eden kullanici
    /// </summary>
    public int VisitorId { get; set; }
    public virtual Visitor Visitor { get; set; } = null!;

    /// <summary>
    /// Sikayet sebebi - ReportReasons.Ids
    /// </summary>
    public int ReasonId { get; set; }

    /// <summary>
    /// Ek aciklama
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Sikayet durumu: 0=Beklemede, 1=Incelendi-Kaldirildi, 2=Incelendi-Reddedildi
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Inceleme tarihi
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Inceleyen yetkili
    /// </summary>
    public int? ReviewedById { get; set; }
    public virtual Visitor? ReviewedBy { get; set; }

    /// <summary>
    /// Inceleme notu
    /// </summary>
    public string ReviewNote { get; set; } = string.Empty;

    /// <summary>
    /// IP adresi
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}
