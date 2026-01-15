using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Tur yorumu ve puanlama - Kapsamli yorum sistemi
/// </summary>
public class TourReview : BaseEntity
{
    // ===============================================
    // TEMEL ALANLAR
    // ===============================================

    /// <summary>
    /// Yorum yapilan tur
    /// </summary>
    public int TourId { get; set; }
    public virtual Tour Tour { get; set; } = null!;

    /// <summary>
    /// Yorumu yazan ziyaretci
    /// </summary>
    public int VisitorId { get; set; }
    public virtual Visitor Visitor { get; set; } = null!;

    /// <summary>
    /// Baglantili rezervasyon (opsiyonel - dogrulanmis yorum icin)
    /// </summary>
    public int? ReservationId { get; set; }
    public virtual Reservation? Reservation { get; set; }

    // ===============================================
    // PUANLAMA - 1-5 arasi
    // ===============================================

    /// <summary>
    /// Genel puan (1-5)
    /// </summary>
    public int OverallRating { get; set; }

    /// <summary>
    /// Hizmet kalitesi puani (1-5)
    /// </summary>
    public int? ServiceRating { get; set; }

    /// <summary>
    /// Fiyat/Deger puani (1-5)
    /// </summary>
    public int? ValueRating { get; set; }

    /// <summary>
    /// Lokasyon puani (1-5)
    /// </summary>
    public int? LocationRating { get; set; }

    /// <summary>
    /// Organizasyon puani (1-5)
    /// </summary>
    public int? OrganizationRating { get; set; }

    /// <summary>
    /// Rehber/Personel puani (1-5)
    /// </summary>
    public int? GuideRating { get; set; }

    // ===============================================
    // YORUM ICERIGI
    // ===============================================

    /// <summary>
    /// Yorum basligi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Olumlu yorumlar - Ne begendiler
    /// </summary>
    public string Pros { get; set; } = string.Empty;

    /// <summary>
    /// Olumsuz yorumlar - Ne begenmediler
    /// </summary>
    public string Cons { get; set; } = string.Empty;

    /// <summary>
    /// Genel yorum/detayli aciklama
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    // ===============================================
    // SEYAHAT BILGILERI
    // ===============================================

    /// <summary>
    /// Seyahat tarihi (tura ne zaman katildi)
    /// </summary>
    public DateTime? VisitDate { get; set; }

    /// <summary>
    /// Seyahat tipi - TravelTypes.Ids
    /// </summary>
    public int TravelTypeId { get; set; } = TravelTypes.Ids.Solo;

    // ===============================================
    // MODERASYON
    // ===============================================

    /// <summary>
    /// Yorum durumu - ReviewStatuses.Ids
    /// </summary>
    public int StatusId { get; set; } = ReviewStatuses.Ids.Pending;

    /// <summary>
    /// Dogrulanmis yorum mu? (Gercekten rezervasyon yapti mi)
    /// </summary>
    public bool IsVerified { get; set; }

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
    /// Moderasyon notu (icsel)
    /// </summary>
    public string ModerationNote { get; set; } = string.Empty;

    /// <summary>
    /// Red sebebi (kullaniciya gosterilir)
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;

    // ===============================================
    // TAVSIYE
    // ===============================================

    /// <summary>
    /// Bu turu tavsiye eder misiniz?
    /// </summary>
    public bool WouldRecommend { get; set; } = true;

    // ===============================================
    // ISTATISTIKLER
    /// </summary>

    /// <summary>
    /// Yardimci bulunma sayisi (pozitif)
    /// </summary>
    public int HelpfulCount { get; set; }

    /// <summary>
    /// Yardimci bulunmama sayisi (negatif)
    /// </summary>
    public int NotHelpfulCount { get; set; }

    /// <summary>
    /// Sikayet sayisi
    /// </summary>
    public int ReportCount { get; set; }

    // ===============================================
    // IP VE CIHAZ (Spam onleme)
    // ===============================================

    /// <summary>
    /// Yorum yapildiginda kullanicinin IP adresi
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Kullanici tarayici/cihaz bilgisi
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    // ===============================================
    // ILISKILER
    // ===============================================

    public virtual ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
    public virtual ICollection<ReviewHelpful> HelpfulVotes { get; set; } = new List<ReviewHelpful>();
    public virtual ICollection<ReviewReply> Replies { get; set; } = new List<ReviewReply>();
}
