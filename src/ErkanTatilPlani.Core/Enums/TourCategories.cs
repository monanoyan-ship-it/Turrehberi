namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Tur kategorileri
/// </summary>
public static class TourCategories
{
    public static readonly TypeItem Adventure = new(0, "Adventure", "TourCategory.Adventure",
        "Macera turlari",
        "bi-compass", "bg-danger", 1, isDefault: true);

    public static readonly TypeItem Culture = new(1, "Culture", "TourCategory.Culture",
        "Kultur ve tarih turlari",
        "bi-bank", "bg-info", 2);

    public static readonly TypeItem Food = new(2, "Food", "TourCategory.Food",
        "Yeme-icme turlari",
        "bi-cup-hot", "bg-warning text-dark", 3);

    public static readonly TypeItem Nature = new(3, "Nature", "TourCategory.Nature",
        "Doga turlari",
        "bi-tree", "bg-success", 4);

    public static readonly TypeItem History = new(4, "History", "TourCategory.History",
        "Tarih turlari",
        "bi-hourglass", "bg-secondary", 5);

    public static readonly TypeItem Beach = new(5, "Beach", "TourCategory.Beach",
        "Deniz ve sahil turlari",
        "bi-water", "bg-primary", 6);

    public static readonly TypeItem City = new(6, "City", "TourCategory.City",
        "Sehir turlari",
        "bi-buildings", "bg-dark", 7);

    public static IEnumerable<TypeItem> All => new[] { Adventure, Culture, Food, Nature, History, Beach, City };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Adventure = 0;
        public const int Culture = 1;
        public const int Food = 2;
        public const int Nature = 3;
        public const int History = 4;
        public const int Beach = 5;
        public const int City = 6;
    }
}
