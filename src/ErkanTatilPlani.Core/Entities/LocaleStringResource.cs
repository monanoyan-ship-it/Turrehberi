namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Ceviri kaynaklari - T("Key") ile erisilir
/// </summary>
public class LocaleStringResource : BaseEntity
{
    /// <summary>
    /// Dil ID
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Kaynak anahtari (orn: "UserType.Visitor", "Common.Save", "Menu.Dashboard")
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Ceviri degeri
    /// </summary>
    public string ResourceValue { get; set; } = string.Empty;

    // Navigation
    public virtual Language Language { get; set; } = null!;
}
