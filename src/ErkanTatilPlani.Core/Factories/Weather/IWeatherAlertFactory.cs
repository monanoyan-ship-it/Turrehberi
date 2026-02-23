using ErkanTatilPlani.Core.Services;

namespace ErkanTatilPlani.Core.Factories.Weather;

public interface IWeatherAlertFactory
{
    Task<WeatherForecast?> GetTourWeatherAsync(int tourId, DateTime date);
    Task<int> CheckUpcomingToursAndNotifyAsync();
}
