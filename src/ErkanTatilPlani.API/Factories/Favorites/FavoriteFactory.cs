using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Favorites;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Favorites;

public class FavoriteFactory : IFavoriteFactory
{
    private readonly IFavoriteEntityService _favoriteService;
    private readonly ITourEntityService _tourService;
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteFactory(IFavoriteEntityService favoriteService, ITourEntityService tourService, IUnitOfWork unitOfWork)
    {
        _favoriteService = favoriteService;
        _tourService = tourService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetFavoritesAsync(int visitorId)
    {
        var favorites = await _favoriteService.GetByVisitorId(visitorId)
            .Include(f => f.Tour)
                .ThenInclude(t => t.Company)
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

        return new { favorites, count = favorites.Count };
    }

    public async Task<bool> CheckFavoriteAsync(int visitorId, int tourId)
        => await _favoriteService.IsFavoriteAsync(visitorId, tourId);

    public async Task<List<int>> CheckMultipleFavoritesAsync(int visitorId, int[] tourIds)
        => await _favoriteService.GetFavoriteTourIdsAsync(visitorId, tourIds);

    public async Task<(bool success, string message)> AddFavoriteAsync(int visitorId, int tourId)
    {
        var tour = await _tourService.GetActiveTours().FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null)
            return (false, "Tur bulunamadi");

        var existingFavorite = await _favoriteService.GetByVisitorAndTourAsync(visitorId, tourId);

        if (existingFavorite != null)
        {
            if (existingFavorite.IsActive)
                return (false, "Bu tur zaten favorilerinizde");

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
            _favoriteService.Add(favorite);
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Tur favorilere eklendi");
    }

    public async Task<(bool success, string message)> RemoveFavoriteAsync(int visitorId, int tourId)
    {
        var favorite = await _favoriteService.GetByVisitorAndTourAsync(visitorId, tourId);
        if (favorite == null || !favorite.IsActive)
            return (false, "Bu tur favorilerinizde degil");

        favorite.IsActive = false;
        favorite.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, "Tur favorilerden cikarildi");
    }

    public async Task<(bool isFavorite, string message, bool tourNotFound)> ToggleFavoriteAsync(int visitorId, int tourId)
    {
        var tour = await _tourService.GetActiveTours().FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null)
            return (false, "Tur bulunamadi", true);

        var favorite = await _favoriteService.GetByVisitorAndTourAsync(visitorId, tourId);
        bool isFavorite;

        if (favorite == null)
        {
            favorite = new FavoriteTour
            {
                VisitorId = visitorId,
                TourId = tourId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _favoriteService.Add(favorite);
            isFavorite = true;
        }
        else
        {
            favorite.IsActive = !favorite.IsActive;
            favorite.UpdatedAt = DateTime.UtcNow;
            isFavorite = favorite.IsActive;
        }

        await _unitOfWork.SaveChangesAsync();
        return (isFavorite, isFavorite ? "Tur favorilere eklendi" : "Tur favorilerden cikarildi", false);
    }
}
