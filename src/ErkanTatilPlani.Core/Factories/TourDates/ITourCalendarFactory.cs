namespace ErkanTatilPlani.Core.Factories.TourDates;

public interface ITourCalendarFactory
{
    Task<(bool success, object result, int statusCode)> GetCalendarDataAsync(int visitorId, int year, int month, int? tourId);
}
