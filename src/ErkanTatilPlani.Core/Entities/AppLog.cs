namespace ErkanTatilPlani.Core.Entities;

/// <summary>
/// Uygulama log kaydi
/// </summary>
public class AppLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Info"; // Debug, Info, Warning, Error, Critical
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? Source { get; set; } // Controller/Service adi
    public string? Action { get; set; } // Method adi
    public string? TraceId { get; set; } // Request trace ID
    public string? UserId { get; set; } // Kullanici ID (varsa)
    public string? UserName { get; set; } // Kullanici adi (varsa)
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public long? ElapsedMs { get; set; } // Islem suresi (ms)
    public string? AdditionalData { get; set; } // JSON formatinda ek veri
}

/// <summary>
/// Log seviyeleri
/// </summary>
public static class LogLevels
{
    public const string Debug = "Debug";
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Error = "Error";
    public const string Critical = "Critical";
}
