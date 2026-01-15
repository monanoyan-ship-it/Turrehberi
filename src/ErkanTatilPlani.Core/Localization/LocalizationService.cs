using System.Text.Json;

namespace ErkanTatilPlani.Core.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _locales = new();
    private readonly string _localesPath;
    private string _currentCulture = "tr";
    private const string DefaultCulture = "tr";

    private static readonly List<LanguageInfo> SupportedLanguages = new()
    {
        new() { Code = "tr", Name = "Turkish", NativeName = "Turkce", IsRtl = false, FlagEmoji = "\ud83c\uddf9\ud83c\uddf7" },
        new() { Code = "en", Name = "English", NativeName = "English", IsRtl = false, FlagEmoji = "\ud83c\uddec\ud83c\udde7" },
        new() { Code = "ru", Name = "Russian", NativeName = "\u0420\u0443\u0441\u0441\u043a\u0438\u0439", IsRtl = false, FlagEmoji = "\ud83c\uddf7\ud83c\uddfa" },
        new() { Code = "de", Name = "German", NativeName = "Deutsch", IsRtl = false, FlagEmoji = "\ud83c\udde9\ud83c\uddea" },
        new() { Code = "es", Name = "Spanish", NativeName = "Espanol", IsRtl = false, FlagEmoji = "\ud83c\uddea\ud83c\uddf8" },
        new() { Code = "fr", Name = "French", NativeName = "Francais", IsRtl = false, FlagEmoji = "\ud83c\uddeb\ud83c\uddf7" },
        new() { Code = "ar", Name = "Arabic", NativeName = "\u0627\u0644\u0639\u0631\u0628\u064a\u0629", IsRtl = true, FlagEmoji = "\ud83c\uddf8\ud83c\udde6" },
        new() { Code = "fa", Name = "Persian", NativeName = "\u0641\u0627\u0631\u0633\u06cc", IsRtl = true, FlagEmoji = "\ud83c\uddee\ud83c\uddf7" },
        new() { Code = "pt", Name = "Portuguese", NativeName = "Portugues", IsRtl = false, FlagEmoji = "\ud83c\udde7\ud83c\uddf7" }
    };

    public LocalizationService(string localesPath)
    {
        _localesPath = localesPath;
        LoadAllLocales();
    }

    public string CurrentCulture => _currentCulture;

    public void SetCulture(string culture)
    {
        if (_locales.ContainsKey(culture))
            _currentCulture = culture;
        else
            _currentCulture = DefaultCulture;
    }

    public string T(string key, params object[] args)
    {
        var value = GetString(key, _currentCulture);

        if (args.Length > 0)
        {
            try
            {
                return string.Format(value, args);
            }
            catch
            {
                return value;
            }
        }

        return value;
    }

    public Dictionary<string, string> GetAllStrings(string? culture = null)
    {
        var targetCulture = culture ?? _currentCulture;

        if (_locales.TryGetValue(targetCulture, out var strings))
            return new Dictionary<string, string>(strings);

        if (_locales.TryGetValue(DefaultCulture, out var defaultStrings))
            return new Dictionary<string, string>(defaultStrings);

        return new Dictionary<string, string>();
    }

    public IEnumerable<LanguageInfo> GetAvailableLanguages()
    {
        return SupportedLanguages.Where(l => _locales.ContainsKey(l.Code));
    }

    private string GetString(string key, string culture)
    {
        // Try requested culture
        if (_locales.TryGetValue(culture, out var cultureStrings))
        {
            if (cultureStrings.TryGetValue(key, out var value))
                return value;
        }

        // Fallback to default culture
        if (culture != DefaultCulture && _locales.TryGetValue(DefaultCulture, out var defaultStrings))
        {
            if (defaultStrings.TryGetValue(key, out var defaultValue))
                return defaultValue;
        }

        // Return key if not found
        return key;
    }

    private void LoadAllLocales()
    {
        if (!Directory.Exists(_localesPath))
            return;

        foreach (var file in Directory.GetFiles(_localesPath, "*.json"))
        {
            var culture = Path.GetFileNameWithoutExtension(file);
            try
            {
                var json = File.ReadAllText(file);
                var strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (strings != null)
                {
                    _locales[culture] = strings;
                }
            }
            catch
            {
                // Skip invalid files
            }
        }
    }

    public void ReloadLocales()
    {
        _locales.Clear();
        LoadAllLocales();
    }
}
