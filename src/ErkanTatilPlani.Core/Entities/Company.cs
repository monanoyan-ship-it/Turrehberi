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

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
