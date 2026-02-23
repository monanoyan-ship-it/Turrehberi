using System.Text.Json;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Weather;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Weather;

public class WeatherAlertFactory : IWeatherAlertFactory
{
    private readonly IWeatherService _weatherService;
    private readonly ITourEntityService _tourService;
    private readonly ITourDateEntityService _tourDateService;
    private readonly IReservationEntityService _reservationService;
    private readonly INotificationEntityService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public WeatherAlertFactory(
        IWeatherService weatherService,
        ITourEntityService tourService,
        ITourDateEntityService tourDateService,
        IReservationEntityService reservationService,
        INotificationEntityService notificationService,
        IUnitOfWork unitOfWork)
    {
        _weatherService = weatherService;
        _tourService = tourService;
        _tourDateService = tourDateService;
        _reservationService = reservationService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<WeatherForecast?> GetTourWeatherAsync(int tourId, DateTime date)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null || !tour.Latitude.HasValue || !tour.Longitude.HasValue)
            return null;

        return await _weatherService.GetForecastAsync(tour.Latitude.Value, tour.Longitude.Value, date);
    }

    public async Task<int> CheckUpcomingToursAndNotifyAsync()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var notificationCount = 0;

        // Tum aktif turlarin ID'lerini al
        var allTourIds = await _tourService.GetActiveTours()
            .Select(t => t.Id)
            .ToListAsync();

        if (!allTourIds.Any())
            return 0;

        // Yarin olan tur tarihlerini bul
        var tomorrowDates = await _tourDateService.GetByTourIds(allTourIds)
            .Where(td => td.StartDate.Date == tomorrow && td.IsActive)
            .Include(td => td.Tour)
            .ToListAsync();

        foreach (var tourDate in tomorrowDates)
        {
            var tour = tourDate.Tour;
            if (tour == null || !tour.Latitude.HasValue || !tour.Longitude.HasValue)
                continue;

            var forecast = await _weatherService.GetForecastAsync(
                tour.Latitude.Value, tour.Longitude.Value, tomorrow);

            if (forecast == null || !forecast.IsRainy)
                continue;

            // Bu tur tarihine rezervasyon yapan kisileri bul
            var reservations = await _reservationService.GetActiveReservations()
                .Where(r => r.TourId == tour.Id &&
                           r.StartDate.Date <= tomorrow &&
                           r.EndDate.Date >= tomorrow &&
                           r.Status == ReservationStatuses.Ids.Confirmed)
                .ToListAsync();

            var messageParams = JsonSerializer.Serialize(new
            {
                tourName = tour.Name,
                temperature = forecast.Temperature,
                condition = forecast.Condition
            });

            foreach (var reservation in reservations)
            {
                _notificationService.Add(new Notification
                {
                    VisitorId = reservation.VisitorId,
                    TitleKey = "Weather.Alert.Title",
                    MessageKey = "Weather.Alert.RainMessage",
                    MessageParams = messageParams,
                    NotificationTypeId = NotificationTypes.Ids.System,
                    RelatedEntityType = "Tour",
                    RelatedEntityId = tour.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                notificationCount++;
            }
        }

        if (notificationCount > 0)
            await _unitOfWork.SaveChangesAsync();

        return notificationCount;
    }
}
