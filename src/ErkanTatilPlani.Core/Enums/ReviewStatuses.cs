namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Yorum durumları - Moderasyon icin
/// </summary>
public static class ReviewStatuses
{
    public static class Ids
    {
        public const int Pending = 0;        // Onay bekliyor
        public const int Approved = 1;       // Onaylandi, gorunur
        public const int Rejected = 2;       // Reddedildi, gizli
        public const int Flagged = 3;        // Sikayet edildi, inceleniyor
    }

    private static readonly List<ReviewStatusItem> _items = new()
    {
        new ReviewStatusItem
        {
            Id = Ids.Pending,
            SystemName = "Pending",
            NameResourceKey = "ReviewStatus.Pending",
            Description = "Yorum onay bekliyor",
            Icon = "bi-hourglass-split",
            CssClass = "warning"
        },
        new ReviewStatusItem
        {
            Id = Ids.Approved,
            SystemName = "Approved",
            NameResourceKey = "ReviewStatus.Approved",
            Description = "Yorum onaylandi",
            Icon = "bi-check-circle",
            CssClass = "success"
        },
        new ReviewStatusItem
        {
            Id = Ids.Rejected,
            SystemName = "Rejected",
            NameResourceKey = "ReviewStatus.Rejected",
            Description = "Yorum reddedildi",
            Icon = "bi-x-circle",
            CssClass = "danger"
        },
        new ReviewStatusItem
        {
            Id = Ids.Flagged,
            SystemName = "Flagged",
            NameResourceKey = "ReviewStatus.Flagged",
            Description = "Yorum sikayet edildi",
            Icon = "bi-flag",
            CssClass = "warning"
        }
    };

    public static IReadOnlyList<ReviewStatusItem> All => _items.AsReadOnly();

    public static ReviewStatusItem? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public static ReviewStatusItem? GetBySystemName(string systemName) =>
        _items.FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
}

public class ReviewStatusItem
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string NameResourceKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
}
