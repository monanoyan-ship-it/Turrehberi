namespace ErkanTatilPlani.Core.Services;

public interface IWeatherService
{
    Task<WeatherForecast?> GetForecastAsync(double latitude, double longitude, DateTime date);
}

public class WeatherForecast
{
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsRainy { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}
