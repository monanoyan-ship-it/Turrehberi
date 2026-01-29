using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Services;

/// <summary>
/// Uygulama log servisi interface'i
/// </summary>
public interface IAppLogService
{
    /// <summary>
    /// Log kaydi ekle
    /// </summary>
    Task LogAsync(AppLog log);

    /// <summary>
    /// Hata logu
    /// </summary>
    Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? action = null, string? additionalData = null);

    /// <summary>
    /// Uyari logu
    /// </summary>
    Task LogWarningAsync(string message, string? source = null, string? action = null, string? additionalData = null);

    /// <summary>
    /// Bilgi logu
    /// </summary>
    Task LogInfoAsync(string message, string? source = null, string? action = null, string? additionalData = null);

    /// <summary>
    /// Debug logu
    /// </summary>
    Task LogDebugAsync(string message, string? source = null, string? action = null, string? additionalData = null);

    /// <summary>
    /// Kritik hata logu
    /// </summary>
    Task LogCriticalAsync(string message, Exception? exception = null, string? source = null, string? action = null, string? additionalData = null);

    /// <summary>
    /// Loglari getir (filtreleme ve sayfalama ile)
    /// </summary>
    Task<LogQueryResult> GetLogsAsync(LogQuery query);

    /// <summary>
    /// Eski loglari temizle
    /// </summary>
    Task<int> CleanupOldLogsAsync(int daysToKeep = 30);
}

/// <summary>
/// Log sorgulama parametreleri
/// </summary>
public class LogQuery
{
    public string? Level { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Source { get; set; }
    public string? UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Log sorgu sonucu
/// </summary>
public class LogQueryResult
{
    public List<AppLog> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
