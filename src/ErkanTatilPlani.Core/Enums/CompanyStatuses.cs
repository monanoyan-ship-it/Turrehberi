namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Firma basvuru durumlari
/// </summary>
public static class CompanyStatuses
{
    public static class Ids
    {
        public const int Pending = 0;      // Basvuru yapildi, onay bekliyor
        public const int Approved = 1;     // Onaylandi, aktif
        public const int Rejected = 2;     // Reddedildi
        public const int Suspended = 3;    // Askiya alindi (sonradan)
    }

    private static readonly List<StatusItem> _items = new()
    {
        new StatusItem
        {
            Id = Ids.Pending,
            SystemName = "Pending",
            NameResourceKey = "CompanyStatus.Pending",
            Description = "Basvuru yapildi, onay bekliyor",
            Icon = "bi-hourglass-split",
            CssClass = "warning"
        },
        new StatusItem
        {
            Id = Ids.Approved,
            SystemName = "Approved",
            NameResourceKey = "CompanyStatus.Approved",
            Description = "Onaylandi, aktif firma",
            Icon = "bi-check-circle",
            CssClass = "success"
        },
        new StatusItem
        {
            Id = Ids.Rejected,
            SystemName = "Rejected",
            NameResourceKey = "CompanyStatus.Rejected",
            Description = "Basvuru reddedildi",
            Icon = "bi-x-circle",
            CssClass = "danger"
        },
        new StatusItem
        {
            Id = Ids.Suspended,
            SystemName = "Suspended",
            NameResourceKey = "CompanyStatus.Suspended",
            Description = "Firma askiya alindi",
            Icon = "bi-pause-circle",
            CssClass = "secondary"
        }
    };

    public static IReadOnlyList<StatusItem> All => _items.AsReadOnly();

    public static StatusItem? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public static StatusItem? GetBySystemName(string systemName) =>
        _items.FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
}

public class StatusItem
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string NameResourceKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
}
