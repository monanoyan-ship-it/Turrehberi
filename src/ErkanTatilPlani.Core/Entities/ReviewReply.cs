using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Yoruma verilen yanitlar - Firma veya diger kullanicilar
/// </summary>
public class ReviewReply : BaseEntity
{
    /// <summary>
    /// Yanitlanan yorum
    /// </summary>
    public int ReviewId { get; set; }
    public virtual TourReview Review { get; set; } = null!;

    /// <summary>
    /// Yaniti yazan kullanici
    /// </summary>
    public int VisitorId { get; set; }
    public virtual Visitor Visitor { get; set; } = null!;

    /// <summary>
    /// Ust yanit (ic ice yanitlar icin, null ise dogrudan yoruma yanit)
    /// </summary>
    public int? ParentReplyId { get; set; }
    public virtual ReviewReply? ParentReply { get; set; }

    /// <summary>
    /// Alt yanitlar
    /// </summary>
    public virtual ICollection<ReviewReply> ChildReplies { get; set; } = new List<ReviewReply>();

    /// <summary>
    /// Yanit icerigi
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Firmadan mi? (Tur saglayici firma temsilcisi)
    /// </summary>
    public bool IsFromCompany { get; set; }

    /// <summary>
    /// Yanit durumu - ReviewStatuses.Ids
    /// </summary>
    public int StatusId { get; set; } = ReviewStatuses.Ids.Approved;

    /// <summary>
    /// Moderasyon tarihi
    /// </summary>
    public DateTime? ModeratedAt { get; set; }

    /// <summary>
    /// Moderasyon yapan yetkili
    /// </summary>
    public int? ModeratedById { get; set; }
    public virtual Visitor? ModeratedBy { get; set; }

    /// <summary>
    /// IP adresi (spam onleme)
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Begeni sayisi
    /// </summary>
    public int LikeCount { get; set; }

    /// <summary>
    /// Sikayet sayisi
    /// </summary>
    public int ReportCount { get; set; }
}
