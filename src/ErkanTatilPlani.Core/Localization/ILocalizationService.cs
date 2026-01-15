namespace ErkanTatilPlani.Core.Localization;

public interface ILocalizationService
{
    string T(string key, params object[] args);
    string CurrentCulture { get; }
    void SetCulture(string culture);
    Dictionary<string, string> GetAllStrings(string? culture = null);
    IEnumerable<LanguageInfo> GetAvailableLanguages();
}

public class LanguageInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsRtl { get; set; }
    public string FlagEmoji { get; set; } = string.Empty;
}
