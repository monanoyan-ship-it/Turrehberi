namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Sikayet sebepleri - Uygunsuz icerik bildirimi icin
/// </summary>
public static class ReportReasons
{
    public static class Ids
    {
        public const int Spam = 0;               // Spam veya reklam
        public const int Inappropriate = 1;      // Uygunsuz icerik
        public const int FakeReview = 2;         // Sahte/yaniltici yorum
        public const int Harassment = 3;         // Taciz/hakaret
        public const int PersonalInfo = 4;       // Kisisel bilgi paylasimi
        public const int OffTopic = 5;           // Konu disi
        public const int CopyrightViolation = 6; // Telif hakki ihlali
        public const int Other = 99;             // Diger
    }

    private static readonly List<ReportReasonItem> _items = new()
    {
        new ReportReasonItem
        {
            Id = Ids.Spam,
            SystemName = "Spam",
            NameResourceKey = "ReportReason.Spam",
            Description = "Spam veya reklam icerigi"
        },
        new ReportReasonItem
        {
            Id = Ids.Inappropriate,
            SystemName = "Inappropriate",
            NameResourceKey = "ReportReason.Inappropriate",
            Description = "Uygunsuz veya muzir icerik"
        },
        new ReportReasonItem
        {
            Id = Ids.FakeReview,
            SystemName = "FakeReview",
            NameResourceKey = "ReportReason.FakeReview",
            Description = "Sahte veya yaniltici yorum"
        },
        new ReportReasonItem
        {
            Id = Ids.Harassment,
            SystemName = "Harassment",
            NameResourceKey = "ReportReason.Harassment",
            Description = "Taciz, hakaret veya nefret soylemi"
        },
        new ReportReasonItem
        {
            Id = Ids.PersonalInfo,
            SystemName = "PersonalInfo",
            NameResourceKey = "ReportReason.PersonalInfo",
            Description = "Kisisel bilgi paylasimi"
        },
        new ReportReasonItem
        {
            Id = Ids.OffTopic,
            SystemName = "OffTopic",
            NameResourceKey = "ReportReason.OffTopic",
            Description = "Tur ile ilgisi olmayan icerik"
        },
        new ReportReasonItem
        {
            Id = Ids.CopyrightViolation,
            SystemName = "CopyrightViolation",
            NameResourceKey = "ReportReason.CopyrightViolation",
            Description = "Telif hakki ihlali"
        },
        new ReportReasonItem
        {
            Id = Ids.Other,
            SystemName = "Other",
            NameResourceKey = "ReportReason.Other",
            Description = "Diger sebepler"
        }
    };

    public static IReadOnlyList<ReportReasonItem> All => _items.AsReadOnly();

    public static ReportReasonItem? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public static ReportReasonItem? GetBySystemName(string systemName) =>
        _items.FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
}

public class ReportReasonItem
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string NameResourceKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
