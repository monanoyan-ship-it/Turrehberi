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
    private readonly IReservationEntityService _reservationService;
    private readonly INotificationEntityService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public WeatherAlertFactory(
        IWeatherService weatherService,
        ITourEntityService tourService,
        IReservationEntityService reservationService,
        INotificationEntityService notificationService,
        IUnitOfWork unitOfWork)
    {
        _weatherService = weatherService;
        _tourService = tourService;
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
        var tomorrowDate = DateOnly.FromDateTime(tomorrow);
        var notificationCount = 0;

        // Yarin icin onaylanmis rezervasyonlari bul
        var tomorrowReservations = await _reservationService.GetActiveReservations()
            .Where(r => r.Date == tomorrowDate &&
                        r.Status == ReservationStatuses.Ids.Confirmed)
            .Include(r => r.Tour)
            .ToListAsync();

        // Turlara gore grupla
        var tourGroups = tomorrowReservations.GroupBy(r => r.TourId);

        foreach (var group in tourGroups)
        {
            var tour = group.First().Tour;
            if (tour == null || !tour.Latitude.HasValue || !tour.Longitude.HasValue)
                continue;

            var forecast = await _weatherService.GetForecastAsync(
                tour.Latitude.Value, tour.Longitude.Value, tomorrow);

            if (forecast == null || !forecast.IsRainy)
                continue;

            var messageParams = JsonSerializer.Serialize(new
            {
                tourName = tour.Name,
                temperature = forecast.Temperature,
                condition = forecast.Condition
            });

            foreach (var reservation in group)
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
