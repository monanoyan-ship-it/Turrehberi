using ErkanTatilPlani.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

/// <summary>
/// Cache yonetim endpoint'leri (sadece Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cache;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ICacheService cache, ILogger<CacheController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Tum cache'i temizle
    /// </summary>
    [HttpPost("clear")]
    public IActionResult ClearAll()
    {
        // Admin kontrolu
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2") // Admin
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        _cache.RemoveByPrefix("tours:");
        _cache.RemoveByPrefix("companies:");
        _cache.RemoveByPrefix("locale:");
        _cache.RemoveByPrefix("stats:");
        _cache.Remove(CacheKeys.Languages);

        _logger.LogInformation("Tum cache temizlendi - Kullanici: {UserId}", User.FindFirst("sub")?.Value);

        return Ok(new { message = "Tum cache temizlendi" });
    }

    /// <summary>
    /// Tur cache'ini temizle
    /// </summary>
    [HttpPost("clear/tours")]
    public IActionResult ClearTours()
    {
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2" && userType != "1") // Admin veya CompanyOwner
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        _cache.RemoveByPrefix(CacheKeys.ToursPrefix);
        _logger.LogInformation("Tur cache temizlendi - Kullanici: {UserId}", User.FindFirst("sub")?.Value);

        return Ok(new { message = "Tur cache temizlendi" });
    }

    /// <summary>
    /// Firma cache'ini temizle
    /// </summary>
    [HttpPost("clear/companies")]
    public IActionResult ClearCompanies()
    {
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2") // Sadece Admin
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        _cache.RemoveByPrefix(CacheKeys.CompaniesPrefix);
        _logger.LogInformation("Firma cache temizlendi - Kullanici: {UserId}", User.FindFirst("sub")?.Value);

        return Ok(new { message = "Firma cache temizlendi" });
    }

    /// <summary>
    /// Localization cache'ini temizle
    /// </summary>
    [HttpPost("clear/localization")]
    public IActionResult ClearLocalization()
    {
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2") // Sadece Admin
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        _cache.RemoveByPrefix("locale:");
        _cache.Remove(CacheKeys.Languages);
        _logger.LogInformation("Localization cache temizlendi - Kullanici: {UserId}", User.FindFirst("sub")?.Value);

        return Ok(new { message = "Localization cache temizlendi" });
    }

    /// <summary>
    /// Istatistik cache'ini temizle
    /// </summary>
    [HttpPost("clear/stats")]
    public IActionResult ClearStats()
    {
        var userType = User.FindFirst("UserType")?.Value;
        if (userType != "2" && userType != "1") // Admin veya CompanyOwner
            return StatusCode(403, new { message = "Bu islem icin yetkiniz yok" });

        _cache.RemoveByPrefix(CacheKeys.StatsPrefix);
        _logger.LogInformation("Istatistik cache temizlendi - Kullanici: {UserId}", User.FindFirst("sub")?.Value);

        return Ok(new { message = "Istatistik cache temizlendi" });
    }
}
