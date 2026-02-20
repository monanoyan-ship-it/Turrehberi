using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.TourWatches;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.TourWatches;

public class TourWatchFactory : ITourWatchFactory
{
    private readonly ITourWatchEntityService _watchService;
    private readonly ITourEntityService _tourService;
    private readonly IUnitOfWork _unitOfWork;

    public TourWatchFactory(ITourWatchEntityService watchService, ITourEntityService tourService, IUnitOfWork unitOfWork)
    {
        _watchService = watchService;
        _tourService = tourService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetWatchedToursAsync(int visitorId)
    {
        var watches = await _watchService.GetByVisitorId(visitorId)
            .Where(w => w.ExpiresAt > DateTime.UtcNow)
            .Include(w => w.Tour)
                .ThenInclude(t => t.Company)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new
            {
                w.Id,
                w.TourId,
                TourName = w.Tour.Name,
                TourDestination = w.Tour.Destination,
                TourPrice = w.Tour.Price,
                TourImageUrl = w.Tour.ImageUrl,
                CompanyName = w.Tour.Company.Name,
                w.WatchDays,
                w.ExpiresAt,
                w.NotifyScarcity,
                w.NotifyPriceChange,
                w.NotifyNewDate,
                w.CreatedAt
            })
            .ToListAsync();

        return new { watches, count = watches.Count };
    }

    public async Task<bool> CheckWatchAsync(int visitorId, int tourId)
    {
        var watch = await _watchService.GetByVisitorAndTourAsync(visitorId, tourId);
        return watch != null && watch.IsActive && watch.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<List<int>> CheckMultipleWatchesAsync(int visitorId, int[] tourIds)
        => await _watchService.GetWatchedTourIdsAsync(visitorId, tourIds);

    public async Task<(bool success, string message)> AddWatchAsync(int visitorId, int tourId, int watchDays)
    {
        if (watchDays < 1 || watchDays > 90)
            return (false, "Takip suresi 1-90 gun arasinda olmalidir");

        var tour = await _tourService.GetActiveTours().FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null)
            return (false, "Tur bulunamadi");

        var existingWatch = await _watchService.GetByVisitorAndTourAsync(visitorId, tourId);

        if (existingWatch != null)
        {
            if (existingWatch.IsActive && existingWatch.ExpiresAt > DateTime.UtcNow)
                return (false, "Bu turu zaten takip ediyorsunuz");

            existingWatch.IsActive = true;
            existingWatch.WatchDays = watchDays;
            existingWatch.ExpiresAt = DateTime.UtcNow.AddDays(watchDays);
            existingWatch.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var watch = new TourWatch
            {
                VisitorId = visitorId,
                TourId = tourId,
                WatchDays = watchDays,
                ExpiresAt = DateTime.UtcNow.AddDays(watchDays),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _watchService.Add(watch);
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, "Tur takibe alindi");
    }

    public async Task<(bool success, string message)> RemoveWatchAsync(int visitorId, int tourId)
    {
        var watch = await _watchService.GetByVisitorAndTourAsync(visitorId, tourId);
        if (watch == null || !watch.IsActive)
            return (false, "Bu turu takip etmiyorsunuz");

        watch.IsActive = false;
        watch.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, "Tur takipten cikarildi");
    }

    public async Task<(bool isWatching, string message, bool tourNotFound)> ToggleWatchAsync(int visitorId, int tourId, int watchDays)
    {
        if (watchDays < 1 || watchDays > 90)
            return (false, "Takip suresi 1-90 gun arasinda olmalidir", false);

        var tour = await _tourService.GetActiveTours().FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour == null)
            return (false, "Tur bulunamadi", true);

        var watch = await _watchService.GetByVisitorAndTourAsync(visitorId, tourId);
        bool isWatching;

        if (watch == null)
        {
            watch = new TourWatch
            {
                VisitorId = visitorId,
                TourId = tourId,
                WatchDays = watchDays,
                ExpiresAt = DateTime.UtcNow.AddDays(watchDays),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _watchService.Add(watch);
            isWatching = true;
        }
        else
        {
            if (watch.IsActive && watch.ExpiresAt > DateTime.UtcNow)
            {
                watch.IsActive = false;
                watch.UpdatedAt = DateTime.UtcNow;
                isWatching = false;
            }
            else
            {
                watch.IsActive = true;
                watch.WatchDays = watchDays;
                watch.ExpiresAt = DateTime.UtcNow.AddDays(watchDays);
                watch.UpdatedAt = DateTime.UtcNow;
                isWatching = true;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (isWatching, isWatching ? "Tur takibe alindi" : "Tur takipten cikarildi", false);
    }
}
