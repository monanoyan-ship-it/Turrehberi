using ErkanTatilPlani.Core.Factories.Weather;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherAlertFactory _weatherAlertFactory;

    public WeatherController(IWeatherAlertFactory weatherAlertFactory)
    {
        _weatherAlertFactory = weatherAlertFactory;
    }

    [HttpGet("tour/{tourId}")]
    public async Task<IActionResult> GetTourWeather(int tourId, [FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var forecast = await _weatherAlertFactory.GetTourWeatherAsync(tourId, targetDate);

        if (forecast == null)
            return NotFound(new { message = "Error.WeatherDataFailed" });

        return Ok(forecast);
    }
}
