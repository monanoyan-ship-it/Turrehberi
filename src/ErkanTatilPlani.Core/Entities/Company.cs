using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    // ===============================================
    // SEO VE PUBLIC PROFIL ALANLARI
    // ===============================================

    /// <summary>
    /// SEO-friendly URL slug (ornek: "mugla-tur-acentasi")
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// SEO Meta Title
    /// </summary>
    public string MetaTitle { get; set; } = string.Empty;

    /// <summary>
    /// SEO Meta Description
    /// </summary>
    public string MetaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Firma slogan/tanitim cumlesi
    /// </summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>
    /// Kurulus yili
    /// </summary>
    public int? FoundedYear { get; set; }

    /// <summary>
    /// Sehir/Bolge (arama ve filtreleme icin)
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Sosyal medya linkleri (JSON formatinda)
    /// </summary>
    public string SocialLinks { get; set; } = string.Empty;

    /// <summary>
    /// Kapak fotografi URL
    /// </summary>
    public string CoverImageUrl { get; set; } = string.Empty;

    // ===============================================
    // BASVURU VE ONAY ALANLARI
    // ===============================================

    /// <summary>
    /// Firma durumu - CompanyStatuses.Ids uzerinden erisim
    /// 0: Pending (Onay bekliyor), 1: Approved (Onaylandi), 2: Rejected (Reddedildi), 3: Suspended (Askiya alindi)
    /// </summary>
    public int StatusId { get; set; } = CompanyStatuses.Ids.Pending;

    /// <summary>
    /// Basvuru tarihi
    /// </summary>
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Basvuru sirasinda firma tarafindan eklenen notlar
    /// </summary>
    public string ApplicationNotes { get; set; } = string.Empty;

    /// <summary>
    /// Onay/Red tarihi
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Onay/Red yapan yetkili (Admin veya Staff) ID
    /// </summary>
    public int? ReviewedById { get; set; }

    /// <summary>
    /// Onay/Red yapan yetkili (Admin veya Staff)
    /// </summary>
    public virtual Visitor? ReviewedBy { get; set; }

    /// <summary>
    /// Admin/Staff tarafindan eklenen dahili notlar
    /// </summary>
    public string ReviewNotes { get; set; } = string.Empty;

    /// <summary>
    /// Red sebebi (sadece StatusId = Rejected ise dolu)
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;

    /// <summary>
    /// Sozlesme dosyasi URL (imzali sozlesme yuklendiginde)
    /// </summary>
    public string ContractFileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Sozlesme yukleme tarihi
    /// </summary>
    public DateTime? ContractUploadedAt { get; set; }

    // ===============================================
    // REZERVASYON AYARLARI
    // ===============================================

    /// <summary>
    /// On odeme yuzdesi (varsayilan %30)
    /// </summary>
    public int DepositPercentage { get; set; } = 30;

    // ===============================================
    // MARKETPLACE VE ODEME AYARLARI
    // ===============================================

    public int SellerLegalTypeId { get; set; } = SellerLegalTypes.Ids.LimitedOrJointStockCompany;
    public int SellerOnboardingStatusId { get; set; } = SellerOnboardingStatuses.Ids.MissingInfo;
    public bool MarketplaceEnabled { get; set; }
    public decimal PlatformCommissionRate { get; set; } = 12;
    public int PayoutDelayDays { get; set; } = 7;
    public string LegalCompanyTitle { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactSurname { get; set; } = string.Empty;
    public string SubMerchantExternalId { get; set; } = string.Empty;
    public string SubMerchantKey { get; set; } = string.Empty;
    public string OnboardingErrorCode { get; set; } = string.Empty;
    public string OnboardingErrorMessage { get; set; } = string.Empty;
    public DateTime? OnboardedAt { get; set; }

    // ===============================================
    // PROMOSYON AYARLARI
    // ===============================================

    public bool EarlyBirdEnabled { get; set; }
    public string? EarlyBirdRules { get; set; }
    public bool GroupDiscountEnabled { get; set; }
    public string? GroupDiscountRules { get; set; }

    // ===============================================

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    public virtual ICollection<CompanyGalleryImage> GalleryImages { get; set; } = new List<CompanyGalleryImage>();
    public virtual ICollection<CompanyPage> Pages { get; set; } = new List<CompanyPage>();
    public virtual ICollection<Guide> Guides { get; set; } = new List<Guide>();
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public virtual ICollection<MessageTemplate> MessageTemplates { get; set; } = new List<MessageTemplate>();
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public virtual ICollection<MarketplaceLedgerEntry> LedgerEntries { get; set; } = new List<MarketplaceLedgerEntry>();
    public virtual ICollection<MarketplaceRefund> MarketplaceRefunds { get; set; } = new List<MarketplaceRefund>();
    public virtual ICollection<PayoutBatch> PayoutBatches { get; set; } = new List<PayoutBatch>();
}
