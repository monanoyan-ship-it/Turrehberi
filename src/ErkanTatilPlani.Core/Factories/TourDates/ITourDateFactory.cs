using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.TourDates;

public interface ITourDateFactory
{
    Task<IEnumerable<object>> GetTourDatesAsync(int tourId);
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> ManageTourDatesAsync(int visitorId, int tourId);
    Task<(TourDate? tourDate, string? errorMessage, string? errorCode, int? statusCode)> CreateTourDateAsync(int visitorId, TourDate tourDate);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdateTourDateAsync(int visitorId, int id, TourDate tourDate);
    Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeleteTourDateAsync(int visitorId, int id);
    Task<IEnumerable<object>> GetCheapestDatesAsync(int tourId, string month);
    Task<(bool success, object result, int statusCode)> GetCapacitySummaryAsync(int visitorId, int tourId);
    Task<(bool success, object? result, int statusCode)> CreateBatchTourDatesAsync(int visitorId, int tourId, BatchTourDateRequest request);
}

public class BatchTourDateRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RepeatEveryDays { get; set; } = 7;
    public List<int>? DaysOfWeek { get; set; }
    /// <summary>
    /// Tur suresi degeri (DurationUnit ile birlikte kullanilir)
    /// </summary>
    public int DurationValue { get; set; } = 1;
    /// <summary>
    /// Tur suresi birimi: hour, day, week, month
    /// </summary>
    public string DurationUnit { get; set; } = "day";
    public decimal? Price { get; set; }
    public int? MaxCapacity { get; set; }
}
