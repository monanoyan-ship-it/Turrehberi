using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.TourDates;

public interface ITourScheduleFactory
{
    Task<(bool success, object result, int statusCode)> GetSchedulesAsync(int visitorId, int tourId);
    Task<(bool success, object result, int statusCode)> CreateScheduleAsync(int visitorId, int tourId, CreateScheduleRequest request);
    Task<(bool success, object result, int statusCode)> UpdateScheduleAsync(int visitorId, int id, UpdateScheduleRequest request);
    Task<(bool success, object result, int statusCode)> DeleteScheduleAsync(int visitorId, int id);
    Task<(bool success, object result, int statusCode)> CancelDateAsync(int visitorId, int tourId, CancelDateRequest request);

    /// <summary>
    /// Schedule'lardan sanal tarih uret + mevcut TourDate'lerle birlestir
    /// </summary>
    Task<List<VirtualTourDateDto>> GenerateVirtualDatesAsync(int tourId, int year, int month);

    /// <summary>
    /// Token'dan TourDate olustur (lazy materialization)
    /// Token formatlari: "s:42:2026-03-15" (schedule) veya "d:123" (mevcut TourDate)
    /// </summary>
    Task<TourDate?> MaterializeDateAsync(string dateToken);
}

public class VirtualTourDateDto
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
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
