using ErkanTatilPlani.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

/// <summary>
/// Log yonetim endpoint'leri (sadece Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class LogsController : ControllerBase
{
    private readonly IAppLogService _logService;

    public LogsController(IAppLogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Loglari listele (filtreleme ve sayfalama ile)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<LogQueryResult>> GetLogs(
        [FromQuery] string? level = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? source = null,
        [FromQuery] string? userId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        // Admin kontrolu
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2") // Admin
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        var query = new LogQuery
        {
            Level = level,
            Search = search,
            FromDate = fromDate,
            ToDate = toDate,
            Source = source,
            UserId = userId,
            Page = page,
            PageSize = Math.Min(pageSize, 100) // Max 100
        };

        var result = await _logService.GetLogsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Log istatistikleri
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2")
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        var today = DateTime.UtcNow.Date;
        var lastWeek = today.AddDays(-7);
        var lastMonth = today.AddDays(-30);

        // Son 24 saat
        var last24HoursQuery = new LogQuery { FromDate = DateTime.UtcNow.AddHours(-24), PageSize = 1 };
        var last24Hours = await _logService.GetLogsAsync(last24HoursQuery);

        // Bugunku hatalar
        var todayErrorsQuery = new LogQuery { Level = "Error", FromDate = today, PageSize = 1 };
        var todayErrors = await _logService.GetLogsAsync(todayErrorsQuery);

        // Bu haftaki hatalar
        var weekErrorsQuery = new LogQuery { Level = "Error", FromDate = lastWeek, PageSize = 1 };
        var weekErrors = await _logService.GetLogsAsync(weekErrorsQuery);

        // Bu ayki hatalar
        var monthErrorsQuery = new LogQuery { Level = "Error", FromDate = lastMonth, PageSize = 1 };
        var monthErrors = await _logService.GetLogsAsync(monthErrorsQuery);

        // Kritik hatalar (son 7 gun)
        var criticalQuery = new LogQuery { Level = "Critical", FromDate = lastWeek, PageSize = 1 };
        var criticals = await _logService.GetLogsAsync(criticalQuery);

        return Ok(new
        {
            last24HoursTotal = last24Hours.TotalCount,
            todayErrors = todayErrors.TotalCount,
            weekErrors = weekErrors.TotalCount,
            monthErrors = monthErrors.TotalCount,
            weekCriticals = criticals.TotalCount
        });
    }

    /// <summary>
    /// Eski loglari temizle
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<object>> Cleanup([FromQuery] int daysToKeep = 30)
    {
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2")
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        if (daysToKeep < 7)
            return BadRequest(new { message = "En az 7 gunluk log tutulmalidir" });

        var deletedCount = await _logService.CleanupOldLogsAsync(daysToKeep);

        return Ok(new { message = $"{deletedCount} adet eski log silindi", deletedCount });
    }
}
