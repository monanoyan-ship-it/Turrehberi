using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Services;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Services;

/// <summary>
/// Veritabani tabanli log servisi
/// </summary>
public class AppLogService : IAppLogService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(AppLog log)
    {
        // HTTP context bilgilerini ekle
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            log.TraceId ??= httpContext.TraceIdentifier;
            log.IpAddress ??= httpContext.Connection.RemoteIpAddress?.ToString();
            log.UserAgent ??= httpContext.Request.Headers["User-Agent"].FirstOrDefault();
            log.RequestPath ??= httpContext.Request.Path;
            log.RequestMethod ??= httpContext.Request.Method;

            // Kullanici bilgileri
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                log.UserId ??= httpContext.User.FindFirst("sub")?.Value ??
                               httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                log.UserName ??= httpContext.User.FindFirst("email")?.Value ??
                                 httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            }
        }

        log.Timestamp = DateTime.UtcNow;

        _context.AppLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? action = null, string? additionalData = null)
    {
        await LogAsync(new AppLog
        {
            Level = LogLevels.Error,
            Message = message,
            Exception = exception?.Message,
            StackTrace = exception?.StackTrace,
            Source = source,
            Action = action,
            AdditionalData = additionalData
        });
    }

    public async Task LogWarningAsync(string message, string? source = null, string? action = null, string? additionalData = null)
    {
        await LogAsync(new AppLog
        {
            Level = LogLevels.Warning,
            Message = message,
            Source = source,
            Action = action,
            AdditionalData = additionalData
        });
    }

    public async Task LogInfoAsync(string message, string? source = null, string? action = null, string? additionalData = null)
    {
        await LogAsync(new AppLog
        {
            Level = LogLevels.Info,
            Message = message,
            Source = source,
            Action = action,
            AdditionalData = additionalData
        });
    }

    public async Task LogDebugAsync(string message, string? source = null, string? action = null, string? additionalData = null)
    {
        await LogAsync(new AppLog
        {
            Level = LogLevels.Debug,
            Message = message,
            Source = source,
            Action = action,
            AdditionalData = additionalData
        });
    }

    public async Task LogCriticalAsync(string message, Exception? exception = null, string? source = null, string? action = null, string? additionalData = null)
    {
        await LogAsync(new AppLog
        {
            Level = LogLevels.Critical,
            Message = message,
            Exception = exception?.Message,
            StackTrace = exception?.StackTrace,
            Source = source,
            Action = action,
            AdditionalData = additionalData
        });
    }

    public async Task<LogQueryResult> GetLogsAsync(LogQuery query)
    {
        var q = _context.AppLogs.AsQueryable();

        // Filtreler
        if (!string.IsNullOrWhiteSpace(query.Level))
            q = q.Where(l => l.Level == query.Level);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(l => l.Message.Contains(query.Search) ||
                            (l.Exception != null && l.Exception.Contains(query.Search)));

        if (query.FromDate.HasValue)
            q = q.Where(l => l.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(l => l.Timestamp <= query.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(query.Source))
            q = q.Where(l => l.Source == query.Source);

        if (!string.IsNullOrWhiteSpace(query.UserId))
            q = q.Where(l => l.UserId == query.UserId);

        // Toplam kayit sayisi
        var totalCount = await q.CountAsync();

        // Siralama ve sayfalama
        var logs = await q
            .OrderByDescending(l => l.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new LogQueryResult
        {
            Logs = logs,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<int> CleanupOldLogsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var deletedCount = await _context.AppLogs
            .Where(l => l.Timestamp < cutoffDate)
            .ExecuteDeleteAsync();

        return deletedCount;
    }
}
