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
}
