namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Blog yazisi kategorileri
/// </summary>
public static class BlogCategories
{
    public static readonly TypeItem TravelTips = new(0, "TravelTips", "Blog.Category.TravelTips",
        "Seyahat ipuclari ve onerileri",
        "bi-lightbulb", "bg-info", 1, isDefault: true);

    public static readonly TypeItem Destinations = new(1, "Destinations", "Blog.Category.Destinations",
        "Gezi rehberleri ve destinasyon tanitimlari",
        "bi-map", "bg-success", 2);

    public static readonly TypeItem News = new(2, "News", "Blog.Category.News",
        "Turizm haberleri ve duyurular",
        "bi-newspaper", "bg-primary", 3);

    public static readonly TypeItem Culture = new(3, "Culture", "Blog.Category.Culture",
        "Kultur ve yasam yazilari",
        "bi-bank", "bg-warning text-dark", 4);

    public static readonly TypeItem FoodAndDrink = new(4, "FoodAndDrink", "Blog.Category.FoodAndDrink",
        "Yeme icme rehberleri",
        "bi-cup-hot", "bg-danger", 5);

    public static readonly TypeItem Adventure = new(5, "Adventure", "Blog.Category.Adventure",
        "Macera ve dogal yasam deneyimleri",
        "bi-tree", "bg-dark", 6);

    public static IEnumerable<TypeItem> All => new[] { TravelTips, Destinations, News, Culture, FoodAndDrink, Adventure };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int TravelTips = 0;
        public const int Destinations = 1;
        public const int News = 2;
        public const int Culture = 3;
        public const int FoodAndDrink = 4;
        public const int Adventure = 5;
    }
}
