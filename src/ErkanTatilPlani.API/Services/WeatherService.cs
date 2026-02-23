using System.Text.Json;
using ErkanTatilPlani.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErkanTatilPlani.API.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<WeatherForecast?> GetForecastAsync(double latitude, double longitude, DateTime date)
    {
        var apiKey = _configuration["Weather:OpenWeatherMapApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("OpenWeatherMap API key is not configured. Returning stub data.");
            return GetStubForecast(latitude, longitude, date);
        }

        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric&lang=tr";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenWeatherMap API returned {StatusCode}", response.StatusCode);
                return GetStubForecast(latitude, longitude, date);
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var list = json.RootElement.GetProperty("list");

            // En yakin tarihi bul
            var targetTimestamp = new DateTimeOffset(date.Date.AddHours(12), TimeSpan.Zero).ToUnixTimeSeconds();
            JsonElement? closestEntry = null;
            long closestDiff = long.MaxValue;

            foreach (var entry in list.EnumerateArray())
            {
                var dt = entry.GetProperty("dt").GetInt64();
                var diff = Math.Abs(dt - targetTimestamp);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closestEntry = entry;
                }
            }

            if (closestEntry == null)
                return GetStubForecast(latitude, longitude, date);

            var temp = closestEntry.Value.GetProperty("main").GetProperty("temp").GetDouble();
            var humidity = closestEntry.Value.GetProperty("main").GetProperty("humidity").GetInt32();
            var windSpeed = closestEntry.Value.GetProperty("wind").GetProperty("speed").GetDouble();
            var weather = closestEntry.Value.GetProperty("weather")[0];
            var condition = weather.GetProperty("description").GetString() ?? "";
            var icon = weather.GetProperty("icon").GetString() ?? "01d";
            var mainWeather = weather.GetProperty("main").GetString() ?? "";
            var isRainy = mainWeather == "Rain" || mainWeather == "Drizzle" || mainWeather == "Thunderstorm";

            var recommendation = GetRecommendation(temp, isRainy, windSpeed);

            return new WeatherForecast
            {
                Temperature = Math.Round(temp, 1),
                Condition = condition,
                Icon = icon,
                IsRainy = isRainy,
                Humidity = humidity,
                WindSpeed = Math.Round(windSpeed, 1),
                Recommendation = recommendation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            return GetStubForecast(latitude, longitude, date);
        }
    }

    private static WeatherForecast GetStubForecast(double latitude, double longitude, DateTime date)
    {
        // Stub data - deterministik
        var seed = (int)(latitude * 100 + longitude * 10 + date.DayOfYear);
        var random = new Random(seed);
        var temp = random.Next(10, 35);
        var isRainy = random.Next(100) < 30;

        return new WeatherForecast
        {
            Temperature = temp,
            Condition = isRainy ? "Hafif yagmur" : "Acik",
            Icon = isRainy ? "10d" : "01d",
            IsRainy = isRainy,
            Humidity = random.Next(30, 80),
            WindSpeed = Math.Round(random.NextDouble() * 15, 1),
            Recommendation = GetRecommendation(temp, isRainy, random.NextDouble() * 15)
        };
    }

    private static string GetRecommendation(double temp, bool isRainy, double windSpeed)
    {
        if (isRainy)
            return "Weather.Recommendation.Rainy";
        if (temp > 35)
            return "Weather.Recommendation.VeryHot";
        if (temp > 28)
            return "Weather.Recommendation.Hot";
        if (temp < 5)
            return "Weather.Recommendation.Cold";
        if (windSpeed > 10)
            return "Weather.Recommendation.Windy";
        return "Weather.Recommendation.Good";
    }
}
