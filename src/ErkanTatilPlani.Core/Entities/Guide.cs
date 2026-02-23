namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Firma rehberi - tur tarihlerine atanabilir
/// </summary>
public class Guide : BaseEntity
{
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Konustugu diller (JSON array: ["tr","en","de"])
    /// </summary>
    public string Languages { get; set; } = "[]";

    /// <summary>
    /// Kisa biyografi
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Deneyim yili
    /// </summary>
    public int? ExperienceYears { get; set; }

    /// <summary>
    /// Tamamlanan tur sayisi (cache)
    /// </summary>
    public int TotalToursCompleted { get; set; }

    /// <summary>
    /// Ortalama puan (cache)
    /// </summary>
    public decimal AverageRating { get; set; }

    public virtual ICollection<TourGuideAssignment> Assignments { get; set; } = new List<TourGuideAssignment>();
}
