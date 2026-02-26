namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Code-based tip tanimlari icin base class
/// Database yerine kod icerisinde tanimlanan tipler icin kullanilir
/// Multi-language destegi icin NameResourceKey kullanilir
/// </summary>
public class TypeItem
{
    public int Id { get; }
    public string SystemName { get; }
    public string NameResourceKey { get; }
    public string? Description { get; }
    public string? Icon { get; }
    public string? CssClass { get; }
    public int DisplayOrder { get; }
    public bool IsDefault { get; }
    public bool IsActive { get; }
    public bool IsSystem { get; }

    public TypeItem(
        int id,
        string systemName,
        string nameResourceKey,
        string? description = null,
        string? icon = null,
        string? cssClass = null,
        int displayOrder = 0,
        bool isDefault = false,
        bool isActive = true,
        bool isSystem = true)
    {
        Id = id;
        SystemName = systemName;
        NameResourceKey = nameResourceKey;
        Description = description;
        Icon = icon;
        CssClass = cssClass;
        DisplayOrder = displayOrder;
        IsDefault = isDefault;
        IsActive = isActive;
        IsSystem = isSystem;
    }
}

// ============================================================
// USER TYPES (Kullanici Tipleri)
// ============================================================
public static class UserTypes
{
    public static readonly TypeItem Visitor = new(0, "Visitor", "UserType.Visitor",
        "Ziyaretci - Tur rezervasyonu yapan normal kullanici",
        "bi-person", "bg-secondary", 1, isDefault: true);

    public static readonly TypeItem CompanyOwner = new(1, "CompanyOwner", "UserType.CompanyOwner",
        "Firma Sahibi - Kendi firmasinin turlarini yonetebilir",
        "bi-building", "bg-info", 2);

    public static readonly TypeItem Staff = new(2, "Staff", "UserType.Staff",
        "Personel - Tum kayitlari gorebilir, sistem ayari yapamaz",
        "bi-person-badge", "bg-warning text-dark", 3);

    public static readonly TypeItem Admin = new(3, "Admin", "UserType.Admin",
        "Sistem Yoneticisi - Tam yetki, sistem ayarlarini yapabilir",
        "bi-shield-fill-check", "bg-danger", 4);

    public static IEnumerable<TypeItem> All => new[] { Visitor, CompanyOwner, Staff, Admin };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Visitor = 0;
        public const int CompanyOwner = 1;
        public const int Staff = 2;
        public const int Admin = 3;
    }
}

// ============================================================
// RESERVATION STATUSES (Rezervasyon Durumlari)
// ============================================================
public static class ReservationStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "ReservationStatus.Pending",
        "Beklemede - Onay bekleniyor",
        "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);

    public static readonly TypeItem Confirmed = new(1, "Confirmed", "ReservationStatus.Confirmed",
        "Onaylandi - Rezervasyon kesinlesti",
        "bi-check-circle", "bg-success", 2);

    public static readonly TypeItem Cancelled = new(2, "Cancelled", "ReservationStatus.Cancelled",
        "Iptal edildi",
        "bi-x-circle", "bg-danger", 3);

    public static readonly TypeItem Completed = new(3, "Completed", "ReservationStatus.Completed",
        "Tamamlandi - Tur gerceklesti",
        "bi-flag-fill", "bg-primary", 4);

    public static IEnumerable<TypeItem> All => new[] { Pending, Confirmed, Cancelled, Completed };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 0;
        public const int Confirmed = 1;
        public const int Cancelled = 2;
        public const int Completed = 3;
    }
}

// ============================================================
// TOUR STATUSES (Tur Durumlari)
// ============================================================
public static class TourStatuses
{
    public static readonly TypeItem Draft = new(0, "Draft", "TourStatus.Draft",
        "Taslak - Henuz yayinlanmadi",
        "bi-file-earmark", "bg-secondary", 1, isDefault: true);

    public static readonly TypeItem Active = new(1, "Active", "TourStatus.Active",
        "Aktif - Rezervasyona acik",
        "bi-check-circle", "bg-success", 2);

    public static readonly TypeItem Paused = new(2, "Paused", "TourStatus.Paused",
        "Duraklatildi - Gecici olarak kapali",
        "bi-pause-circle", "bg-warning text-dark", 3);

    public static readonly TypeItem Cancelled = new(3, "Cancelled", "TourStatus.Cancelled",
        "Iptal edildi",
        "bi-x-circle", "bg-danger", 4);

    public static readonly TypeItem Completed = new(4, "Completed", "TourStatus.Completed",
        "Tamamlandi - Tur gerceklesti",
        "bi-flag-fill", "bg-primary", 5);

    public static IEnumerable<TypeItem> All => new[] { Draft, Active, Paused, Cancelled, Completed };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Draft = 0;
        public const int Active = 1;
        public const int Paused = 2;
        public const int Cancelled = 3;
        public const int Completed = 4;
    }
}

// ============================================================
// PAYMENT STATUSES (Odeme Durumlari)
// ============================================================
public static class PaymentStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "PaymentStatus.Pending",
        "Odeme bekliyor",
        "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);

    public static readonly TypeItem DepositPaid = new(1, "DepositPaid", "PaymentStatus.DepositPaid",
        "On odeme yapildi",
        "bi-cash-stack", "bg-info", 2);

    public static readonly TypeItem FullyPaid = new(2, "FullyPaid", "PaymentStatus.FullyPaid",
        "Tam odeme yapildi",
        "bi-check-circle-fill", "bg-success", 3);

    public static readonly TypeItem Failed = new(3, "Failed", "PaymentStatus.Failed",
        "Odeme basarisiz",
        "bi-x-circle", "bg-danger", 4);

    public static readonly TypeItem Refunded = new(4, "Refunded", "PaymentStatus.Refunded",
        "Iade edildi",
        "bi-arrow-counterclockwise", "bg-secondary", 5);

    public static IEnumerable<TypeItem> All => new[] { Pending, DepositPaid, FullyPaid, Failed, Refunded };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 0;
        public const int DepositPaid = 1;
        public const int FullyPaid = 2;
        public const int Failed = 3;
        public const int Refunded = 4;
    }
}

// ============================================================
// NOTIFICATION TYPES (Bildirim Tipleri)
// ============================================================
public static class NotificationTypes
{
    public static readonly TypeItem Scarcity = new(0, "Scarcity", "NotificationType.Scarcity",
        "Son X yer kaldi bildirimi",
        "bi-exclamation-triangle", "bg-warning text-dark", 1);

    public static readonly TypeItem PriceChange = new(1, "PriceChange", "NotificationType.PriceChange",
        "Fiyat degisikligi bildirimi",
        "bi-graph-down-arrow", "bg-info", 2);

    public static readonly TypeItem NewDate = new(2, "NewDate", "NotificationType.NewDate",
        "Yeni tarih eklendi bildirimi",
        "bi-calendar-plus", "bg-success", 3);

    public static readonly TypeItem Reservation = new(3, "Reservation", "NotificationType.Reservation",
        "Rezervasyon bildirimi",
        "bi-calendar-check", "bg-primary", 4);

    public static readonly TypeItem System = new(4, "System", "NotificationType.System",
        "Genel sistem bildirimi",
        "bi-bell", "bg-secondary", 5);

    public static readonly TypeItem NewFollower = new(5, "NewFollower", "NotificationType.NewFollower",
        "Yeni takipci bildirimi",
        "bi-person-plus", "bg-info", 6);

    public static readonly TypeItem StoryLiked = new(6, "StoryLiked", "NotificationType.StoryLiked",
        "Hikaye begenildi bildirimi",
        "bi-heart-fill", "bg-danger", 7);

    public static readonly TypeItem StoryCommented = new(7, "StoryCommented", "NotificationType.StoryCommented",
        "Hikayeye yorum yapildi bildirimi",
        "bi-chat-dots", "bg-primary", 8);

    public static IEnumerable<TypeItem> All => new[] { Scarcity, PriceChange, NewDate, Reservation, System, NewFollower, StoryLiked, StoryCommented };
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Scarcity = 0;
        public const int PriceChange = 1;
        public const int NewDate = 2;
        public const int Reservation = 3;
        public const int System = 4;
        public const int NewFollower = 5;
        public const int StoryLiked = 6;
        public const int StoryCommented = 7;
    }
}

// ============================================================
// PROMOTION TYPES (Promosyon Tipleri)
// ============================================================
public static class PromotionTypes
{
    public static readonly TypeItem Coupon = new(0, "Coupon", "PromotionType.Coupon",
        "Kupon kodu ile indirim",
        "bi-ticket-perforated", "bg-primary", 1);

    public static readonly TypeItem EarlyBird = new(1, "EarlyBird", "PromotionType.EarlyBird",
        "Erken rezervasyon indirimi",
        "bi-alarm", "bg-info", 2);

    public static readonly TypeItem LastMinute = new(2, "LastMinute", "PromotionType.LastMinute",
        "Son dakika firsati",
        "bi-lightning", "bg-warning text-dark", 3);

    public static readonly TypeItem GroupDiscount = new(3, "GroupDiscount", "PromotionType.GroupDiscount",
        "Grup indirimi",
        "bi-people", "bg-success", 4);

    public static readonly TypeItem FlashSale = new(4, "FlashSale", "PromotionType.FlashSale",
        "Flash sale - sinirli sureli kampanya",
        "bi-stopwatch", "bg-danger", 5);

    public static readonly TypeItem Bundle = new(5, "Bundle", "PromotionType.Bundle",
        "Paket fiyatlandirma",
        "bi-box-seam", "bg-dark", 6);

    public static readonly TypeItem DynamicPricing = new(6, "DynamicPricing", "PromotionType.DynamicPricing",
        "Dinamik fiyatlandirma",
        "bi-graph-up-arrow", "bg-secondary", 7);

    public static IEnumerable<TypeItem> All => new[] { Coupon, EarlyBird, LastMinute, GroupDiscount, FlashSale, Bundle, DynamicPricing };
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Coupon = 0;
        public const int EarlyBird = 1;
        public const int LastMinute = 2;
        public const int GroupDiscount = 3;
        public const int FlashSale = 4;
        public const int Bundle = 5;
        public const int DynamicPricing = 6;
    }
}

// ============================================================
// DISCOUNT TYPES (Indirim Tipleri)
// ============================================================
public static class DiscountTypes
{
    public static readonly TypeItem Percentage = new(0, "Percentage", "DiscountType.Percentage",
        "Yuzdelik indirim",
        "bi-percent", "bg-primary", 1, isDefault: true);

    public static readonly TypeItem FixedAmount = new(1, "FixedAmount", "DiscountType.FixedAmount",
        "Sabit tutar indirimi",
        "bi-currency-lira", "bg-success", 2);

    public static readonly TypeItem Multiplier = new(2, "Multiplier", "DiscountType.Multiplier",
        "Carpan (dinamik fiyatlandirma icin)",
        "bi-x-diamond", "bg-info", 3);

    public static IEnumerable<TypeItem> All => new[] { Percentage, FixedAmount, Multiplier };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Percentage = 0;
        public const int FixedAmount = 1;
        public const int Multiplier = 2;
    }
}

// ============================================================
// PROMOTION STATUSES (Promosyon Durumlari)
// ============================================================
public static class PromotionStatuses
{
    public static readonly TypeItem Active = new(0, "Active", "PromotionStatus.Active",
        "Aktif promosyon",
        "bi-check-circle", "bg-success", 1, isDefault: true);

    public static readonly TypeItem Disabled = new(1, "Disabled", "PromotionStatus.Disabled",
        "Devre disi birakilmis",
        "bi-pause-circle", "bg-secondary", 2);

    public static readonly TypeItem Expired = new(2, "Expired", "PromotionStatus.Expired",
        "Suresi dolmus",
        "bi-clock-history", "bg-warning text-dark", 3);

    public static IEnumerable<TypeItem> All => new[] { Active, Disabled, Expired };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Active = 0;
        public const int Disabled = 1;
        public const int Expired = 2;
    }
}

// ============================================================
// MESSAGE SENDER TYPES (Mesaj Gonderen Tipleri)
// ============================================================
public static class MessageSenderTypes
{
    public static readonly TypeItem Visitor = new(0, "Visitor", "MessageSenderType.Visitor",
        "Ziyaretci/Turist",
        "bi-person", "bg-info", 1, isDefault: true);

    public static readonly TypeItem Company = new(1, "Company", "MessageSenderType.Company",
        "Firma",
        "bi-building", "bg-primary", 2);

    public static IEnumerable<TypeItem> All => new[] { Visitor, Company };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Visitor = 0;
        public const int Company = 1;
    }
}

// ============================================================
// GUIDE ASSIGNMENT STATUSES (Rehber Atama Durumlari)
// ============================================================
public static class GuideAssignmentStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "GuideAssignmentStatus.Pending",
        "Onay bekliyor",
        "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);

    public static readonly TypeItem Confirmed = new(1, "Confirmed", "GuideAssignmentStatus.Confirmed",
        "Onaylandi",
        "bi-check-circle", "bg-success", 2);

    public static readonly TypeItem Rejected = new(2, "Rejected", "GuideAssignmentStatus.Rejected",
        "Reddedildi",
        "bi-x-circle", "bg-danger", 3);

    public static IEnumerable<TypeItem> All => new[] { Pending, Confirmed, Rejected };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 0;
        public const int Confirmed = 1;
        public const int Rejected = 2;
    }
}

// ============================================================
// LOYALTY TIERS (Sadakat Kademeleri)
// ============================================================
public static class LoyaltyTiers
{
    public static readonly TypeItem Explorer = new(0, "Explorer", "LoyaltyTier.Explorer",
        "Kasif - Baslangic kademesi, %5 indirim",
        "bi-compass", "bg-secondary", 1, isDefault: true);

    public static readonly TypeItem Traveler = new(1, "Traveler", "LoyaltyTier.Traveler",
        "Gezgin - Orta kademe, %10 indirim",
        "bi-backpack2", "bg-info", 2);

    public static readonly TypeItem WorldTraveler = new(2, "WorldTraveler", "LoyaltyTier.WorldTraveler",
        "Dunya Gezgini - En ust kademe, %15 indirim",
        "bi-globe-americas", "bg-warning text-dark", 3);

    public static IEnumerable<TypeItem> All => new[] { Explorer, Traveler, WorldTraveler };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static int GetDiscountPercent(int tierId) => tierId switch
    {
        Ids.Explorer => 5,
        Ids.Traveler => 10,
        Ids.WorldTraveler => 15,
        _ => 0
    };

    public static int GetCreditEarnPercent(int tierId) => tierId switch
    {
        Ids.Explorer => 2,
        Ids.Traveler => 3,
        Ids.WorldTraveler => 5,
        _ => 0
    };

    public static int GetMinCompletedTours(int tierId) => tierId switch
    {
        Ids.Explorer => 0,
        Ids.Traveler => 3,
        Ids.WorldTraveler => 10,
        _ => 0
    };

    public static class Ids
    {
        public const int Explorer = 0;
        public const int Traveler = 1;
        public const int WorldTraveler = 2;
    }
}

// ============================================================
// CREDIT TRANSACTION TYPES (Kredi Islem Tipleri)
// ============================================================
public static class CreditTransactionTypes
{
    public static readonly TypeItem Earn = new(0, "Earn", "CreditTransactionType.Earn",
        "Kredi kazanma",
        "bi-plus-circle", "bg-success", 1);

    public static readonly TypeItem Spend = new(1, "Spend", "CreditTransactionType.Spend",
        "Kredi harcama",
        "bi-dash-circle", "bg-danger", 2);

    public static readonly TypeItem Expire = new(2, "Expire", "CreditTransactionType.Expire",
        "Kredi suresi doldu",
        "bi-clock-history", "bg-warning text-dark", 3);

    public static readonly TypeItem ReferralBonus = new(3, "ReferralBonus", "CreditTransactionType.ReferralBonus",
        "Referans bonusu",
        "bi-gift", "bg-primary", 4);

    public static IEnumerable<TypeItem> All => new[] { Earn, Spend, Expire, ReferralBonus };
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Earn = 0;
        public const int Spend = 1;
        public const int Expire = 2;
        public const int ReferralBonus = 3;
    }
}

// ============================================================
// REFERRAL STATUSES (Referans Durumlari)
// ============================================================
public static class ReferralStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "ReferralStatus.Pending",
        "Beklemede - Kayit yapildi ama henuz rezervasyon yok",
        "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);

    public static readonly TypeItem Completed = new(1, "Completed", "ReferralStatus.Completed",
        "Tamamlandi - Davet edilen kisi ilk rezervasyonunu yapti",
        "bi-check-circle", "bg-success", 2);

    public static readonly TypeItem Rewarded = new(2, "Rewarded", "ReferralStatus.Rewarded",
        "Odullendirildi - Her iki tarafa kredi verildi",
        "bi-trophy", "bg-primary", 3);

    public static IEnumerable<TypeItem> All => new[] { Pending, Completed, Rewarded };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 0;
        public const int Completed = 1;
        public const int Rewarded = 2;
    }
}

// ============================================================
// SCHEDULED EMAIL TYPES (Zamanlanmis Email Tipleri)
// ============================================================
public static class ScheduledEmailTypes
{
    public static readonly TypeItem TourDetailsReminder = new(0, "TourDetailsReminder", "ScheduledEmailType.TourDetailsReminder",
        "Tur detay hatirlatmasi - 3 gun once",
        "bi-envelope", "bg-info", 1);

    public static readonly TypeItem DayBeforeReminder = new(1, "DayBeforeReminder", "ScheduledEmailType.DayBeforeReminder",
        "Bir gun oncesi hatirlatmasi",
        "bi-calendar-event", "bg-primary", 2);

    public static readonly TypeItem TourMorning = new(2, "TourMorning", "ScheduledEmailType.TourMorning",
        "Tur sabahi bildirimi",
        "bi-sunrise", "bg-warning text-dark", 3);

    public static readonly TypeItem ThankYou = new(3, "ThankYou", "ScheduledEmailType.ThankYou",
        "Tesekkur emaili - tur sonrasi",
        "bi-heart", "bg-success", 4);

    public static readonly TypeItem SimilarTourRecommendation = new(4, "SimilarTourRecommendation", "ScheduledEmailType.SimilarTourRecommendation",
        "Benzer tur onerisi - 7 gun sonra",
        "bi-stars", "bg-secondary", 5);

    public static readonly TypeItem AbandonedCart = new(5, "AbandonedCart", "ScheduledEmailType.AbandonedCart",
        "Terk edilmis sepet hatirlatmasi",
        "bi-cart-x", "bg-danger", 6);

    public static IEnumerable<TypeItem> All => new[] { TourDetailsReminder, DayBeforeReminder, TourMorning, ThankYou, SimilarTourRecommendation, AbandonedCart };
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int TourDetailsReminder = 0;
        public const int DayBeforeReminder = 1;
        public const int TourMorning = 2;
        public const int ThankYou = 3;
        public const int SimilarTourRecommendation = 4;
        public const int AbandonedCart = 5;
    }
}

// ============================================================
// SCHEDULED EMAIL STATUSES (Zamanlanmis Email Durumlari)
// ============================================================
public static class ScheduledEmailStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "ScheduledEmailStatus.Pending",
        "Beklemede - Gonderim zamani gelmedi",
        "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);

    public static readonly TypeItem Sent = new(1, "Sent", "ScheduledEmailStatus.Sent",
        "Gonderildi",
        "bi-check-circle", "bg-success", 2);

    public static readonly TypeItem Failed = new(2, "Failed", "ScheduledEmailStatus.Failed",
        "Gonderilemedi",
        "bi-x-circle", "bg-danger", 3);

    public static readonly TypeItem Cancelled = new(3, "Cancelled", "ScheduledEmailStatus.Cancelled",
        "Iptal edildi",
        "bi-slash-circle", "bg-secondary", 4);

    public static IEnumerable<TypeItem> All => new[] { Pending, Sent, Failed, Cancelled };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 0;
        public const int Sent = 1;
        public const int Failed = 2;
        public const int Cancelled = 3;
    }
}

// ============================================================
// DURATION UNITS (Sure Birimleri)
// ============================================================
public static class DurationUnits
{
    public static readonly TypeItem Hour = new(1, "Hour", "TourDate.DurationHour",
        "Saat",
        "bi-clock", null, 1);

    public static readonly TypeItem Day = new(2, "Day", "TourDate.DurationDay",
        "Gun",
        "bi-calendar-day", null, 2, isDefault: true);

    public static readonly TypeItem Week = new(3, "Week", "TourDate.DurationWeek",
        "Hafta",
        "bi-calendar-week", null, 3);

    public static readonly TypeItem Month = new(4, "Month", "TourDate.DurationMonth",
        "Ay",
        "bi-calendar-month", null, 4);

    public static IEnumerable<TypeItem> All => new[] { Hour, Day, Week, Month };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

    public static class Ids
    {
        public const int Hour = 1;
        public const int Day = 2;
        public const int Week = 3;
        public const int Month = 4;
    }
}
