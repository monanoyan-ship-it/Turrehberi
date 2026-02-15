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
