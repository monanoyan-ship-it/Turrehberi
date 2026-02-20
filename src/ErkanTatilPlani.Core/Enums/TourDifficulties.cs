namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Tur zorluk seviyeleri
/// </summary>
public static class TourDifficulties
{
    public static readonly TypeItem Easy = new(0, "Easy", "TourDifficulty.Easy",
        "Kolay - Her yasa uygun",
        "bi-emoji-smile", "bg-success", 1);

    public static readonly TypeItem Moderate = new(1, "Moderate", "TourDifficulty.Moderate",
        "Orta - Orta duzey fiziksel aktivite",
        "bi-emoji-neutral", "bg-info", 2, isDefault: true);

    public static readonly TypeItem Challenging = new(2, "Challenging", "TourDifficulty.Challenging",
        "Zor - Fiziksel kondisyon gerektirir",
        "bi-emoji-frown", "bg-warning text-dark", 3);

    public static readonly TypeItem Expert = new(3, "Expert", "TourDifficulty.Expert",
        "Uzman - Deneyim ve ekipman gerektirir",
        "bi-exclamation-triangle", "bg-danger", 4);

    public static IEnumerable<TypeItem> All => new[] { Easy, Moderate, Challenging, Expert };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Easy = 0;
        public const int Moderate = 1;
        public const int Challenging = 2;
        public const int Expert = 3;
    }
}
