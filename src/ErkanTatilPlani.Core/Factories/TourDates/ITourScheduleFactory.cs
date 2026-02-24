namespace ErkanTatilPlani.Core.Factories.TourDates;

public interface ITourScheduleFactory
{
    Task<(bool success, object result, int statusCode)> GetSchedulesAsync(int visitorId, int tourId);
    Task<(bool success, object result, int statusCode)> CreateScheduleAsync(int visitorId, int tourId, CreateScheduleRequest request);
    Task<(bool success, object result, int statusCode)> UpdateScheduleAsync(int visitorId, int id, UpdateScheduleRequest request);
    Task<(bool success, object result, int statusCode)> DeleteScheduleAsync(int visitorId, int id);
    Task<(bool success, object result, int statusCode)> CancelDateAsync(int visitorId, int tourId, CancelDateRequest request);

    /// <summary>
    /// Schedule'lardan sanal tarih uret (reservation count ile doluluk hesabi)
    /// </summary>
    Task<List<VirtualTourDateDto>> GenerateVirtualDatesAsync(int tourId, int year, int month);

    /// <summary>
    /// Public: Tur icin onumuzdeki 3 ayin musait tarihlerini getir
    /// </summary>
    Task<IEnumerable<object>> GetTourDatesAsync(int tourId);

    /// <summary>
    /// Public: En ucuz tarihleri getir
    /// </summary>
    Task<IEnumerable<object>> GetCheapestDatesAsync(int tourId, string month);

    /// <summary>
    /// Firma: Takvim verisini getir
    /// </summary>
    Task<(bool success, object result, int statusCode)> GetCalendarDataAsync(int visitorId, int year, int month, int? tourId);
}

public class VirtualTourDateDto
{
    public int TourId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationValue { get; set; }
    public int DurationUnitId { get; set; }
    public decimal? Price { get; set; }
    public int? MaxCapacity { get; set; }
    public int BookedCount { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsCancelled { get; set; }
    public string Token { get; set; } = string.Empty;
    public int? ScheduleId { get; set; }
    public bool IsVirtual { get; set; }
}

public class CreateScheduleRequest
{
    public List<int> DaysOfWeek { get; set; } = new();
    public string StartTime { get; set; } = "09:00";
    public int DurationValue { get; set; } = 1;
    public string DurationUnit { get; set; } = "Day";
    public decimal? Price { get; set; }
    public int? MaxCapacity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class UpdateScheduleRequest
{
    public List<int>? DaysOfWeek { get; set; }
    public string? StartTime { get; set; }
    public int? DurationValue { get; set; }
    public string? DurationUnit { get; set; }
    public decimal? Price { get; set; }
    public int? MaxCapacity { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class CancelDateRequest
{
    public string DateToken { get; set; } = string.Empty;
}
