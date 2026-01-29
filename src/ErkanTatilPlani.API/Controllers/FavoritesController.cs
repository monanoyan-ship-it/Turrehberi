using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FavoritesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kullanicinin favori turlarini listele
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetFavorites()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var favorites = await _context.FavoriteTours
            .Include(f => f.Tour)
                .ThenInclude(t => t.Company)
            .Where(f => f.VisitorId == visitorId && f.IsActive)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.Id,
                f.TourId,
                TourName = f.Tour.Name,
                TourDescription = f.Tour.Description,
                TourDestination = f.Tour.Destination,
                TourPrice = f.Tour.Price,
                TourDurationDays = f.Tour.DurationDays,
                TourImageUrl = f.Tour.ImageUrl,
                TourIsFeatured = f.Tour.IsFeatured,
                TourAverageRating = f.Tour.AverageRating,
                TourReviewCount = f.Tour.ReviewCount,
                CompanyId = f.Tour.Company.Id,
                CompanyName = f.Tour.Company.Name,
                CompanySlug = f.Tour.Company.Slug,
                f.CreatedAt
            })
            .ToListAsync();

        return Ok(new { favorites, count = favorites.Count });
    }

    /// <summary>
    /// Tur favori mi kontrol et
    /// </summary>
    [HttpGet("check/{tourId}")]
    public async Task<ActionResult<object>> CheckFavorite(int tourId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Ok(new { isFavorite = false });

        var visitorId = int.Parse(userIdClaim);

        var isFavorite = await _context.FavoriteTours
            .AnyAsync(f => f.VisitorId == visitorId && f.TourId == tourId && f.IsActive);

        return Ok(new { isFavorite });
    }

    /// <summary>
    /// Birden fazla turun favori durumunu kontrol et
    /// </summary>
    [HttpPost("check-multiple")]
    public async Task<ActionResult<object>> CheckMultipleFavorites([FromBody] int[] tourIds)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Ok(new { favoriteIds = Array.Empty<int>() });

        var visitorId = int.Parse(userIdClaim);

        var favoriteIds = await _context.FavoriteTours
            .Where(f => f.VisitorId == visitorId && tourIds.Contains(f.TourId) && f.IsActive)
            .Select(f => f.TourId)
            .ToListAsync();

        return Ok(new { favoriteIds });
    }

    /// <summary>
    /// Turu favorilere ekle
    /// </summary>
    [HttpPost("{tourId}")]
    public async Task<IActionResult> AddFavorite(int tourId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        // Tur var mi kontrol et
        var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == tourId && t.IsActive);
        if (tour == null)
            return NotFound(new { message = "Tur bulunamadi" });

        // Zaten favorilerde mi kontrol et
        var existingFavorite = await _context.FavoriteTours
            .FirstOrDefaultAsync(f => f.VisitorId == visitorId && f.TourId == tourId);

        if (existingFavorite != null)
        {
            if (existingFavorite.IsActive)
                return BadRequest(new { message = "Bu tur zaten favorilerinizde" });

            // Soft delete'ten geri getir
            existingFavorite.IsActive = true;
            existingFavorite.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var favorite = new FavoriteTour
            {
                VisitorId = visitorId,
                TourId = tourId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.FavoriteTours.Add(favorite);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Tur favorilere eklendi" });
    }

    /// <summary>
    /// Turu favorilerden cikar
    /// </summary>
    [HttpDelete("{tourId}")]
    public async Task<IActionResult> RemoveFavorite(int tourId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var favorite = await _context.FavoriteTours
            .FirstOrDefaultAsync(f => f.VisitorId == visitorId && f.TourId == tourId && f.IsActive);

        if (favorite == null)
            return NotFound(new { message = "Bu tur favorilerinizde degil" });

        // Soft delete
        favorite.IsActive = false;
        favorite.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Tur favorilerden cikarildi" });
    }

    /// <summary>
    /// Favori durumunu toggle et (ekle/cikar)
    /// </summary>
    [HttpPost("{tourId}/toggle")]
    public async Task<ActionResult<object>> ToggleFavorite(int tourId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        // Tur var mi kontrol et
        var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == tourId && t.IsActive);
        if (tour == null)
            return NotFound(new { message = "Tur bulunamadi" });

        var favorite = await _context.FavoriteTours
            .FirstOrDefaultAsync(f => f.VisitorId == visitorId && f.TourId == tourId);

        bool isFavorite;

        if (favorite == null)
        {
            // Yeni favori ekle
            favorite = new FavoriteTour
            {
                VisitorId = visitorId,
                TourId = tourId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.FavoriteTours.Add(favorite);
            isFavorite = true;
        }
        else
        {
            // Toggle
            favorite.IsActive = !favorite.IsActive;
            favorite.UpdatedAt = DateTime.UtcNow;
            isFavorite = favorite.IsActive;
        }

        await _context.SaveChangesAsync();

        return Ok(new {
            isFavorite,
            message = isFavorite ? "Tur favorilere eklendi" : "Tur favorilerden cikarildi"
        });
    }
}
