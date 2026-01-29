namespace ErkanTatilPlani.Core.Services;

/// <summary>
/// Cache servisi interface'i
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den veri al
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Cache'e veri ekle
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Cache'den veri sil
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Belirli bir pattern'e uyan tüm cache'leri sil
    /// </summary>
    void RemoveByPrefix(string prefix);

    /// <summary>
    /// Cache'de var mı kontrol et
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// Cache'den al veya oluştur
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

/// <summary>
/// Cache key sabitleri
/// </summary>
public static class CacheKeys
{
    // Tours
    public const string AllTours = "tours:all";
    public const string FeaturedTours = "tours:featured";
    public const string TourById = "tours:id:{0}";
    public const string ToursByCompany = "tours:company:{0}";

    // Companies
    public const string AllCompanies = "companies:all";
    public const string ApprovedCompanies = "companies:approved";
    public const string CompanyById = "companies:id:{0}";
    public const string CompanyBySlug = "companies:slug:{0}";

    // Localization
    public const string LocaleByLang = "locale:{0}";
    public const string Languages = "languages";

    // Statistics
    public const string HomeStats = "stats:home";
    public const string CompanyStats = "stats:company:{0}";

    // Prefixes for bulk invalidation
    public const string ToursPrefix = "tours:";
    public const string CompaniesPrefix = "companies:";
    public const string StatsPrefix = "stats:";
}

/// <summary>
/// Cache süreleri
/// </summary>
public static class CacheDurations
{
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Long = TimeSpan.FromHours(1);
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);
}
