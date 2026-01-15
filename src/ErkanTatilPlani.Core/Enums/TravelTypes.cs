namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Seyahat tipleri - Yorumda kullanici seyahat sekli
/// </summary>
public static class TravelTypes
{
    public static class Ids
    {
        public const int Solo = 0;           // Yalniz
        public const int Couple = 1;         // Cift
        public const int Family = 2;         // Aile
        public const int Friends = 3;        // Arkadaslar
        public const int Business = 4;       // Is seyahati
    }

    private static readonly List<TravelTypeItem> _items = new()
    {
        new TravelTypeItem
        {
            Id = Ids.Solo,
            SystemName = "Solo",
            NameResourceKey = "TravelType.Solo",
            Icon = "bi-person"
        },
        new TravelTypeItem
        {
            Id = Ids.Couple,
            SystemName = "Couple",
            NameResourceKey = "TravelType.Couple",
            Icon = "bi-heart"
        },
        new TravelTypeItem
        {
            Id = Ids.Family,
            SystemName = "Family",
            NameResourceKey = "TravelType.Family",
            Icon = "bi-people"
        },
        new TravelTypeItem
        {
            Id = Ids.Friends,
            SystemName = "Friends",
            NameResourceKey = "TravelType.Friends",
            Icon = "bi-person-hearts"
        },
        new TravelTypeItem
        {
            Id = Ids.Business,
            SystemName = "Business",
            NameResourceKey = "TravelType.Business",
            Icon = "bi-briefcase"
        }
    };

    public static IReadOnlyList<TravelTypeItem> All => _items.AsReadOnly();

    public static TravelTypeItem? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public static TravelTypeItem? GetBySystemName(string systemName) =>
        _items.FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
}

public class TravelTypeItem
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string NameResourceKey { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
