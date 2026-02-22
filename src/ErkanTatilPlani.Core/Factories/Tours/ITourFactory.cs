using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Tours;

public interface ITourFactory
{
    Task<object> GetToursAsync(string? search, string? destination, decimal? minPrice, decimal? maxPrice, int? minDays, int? maxDays, int? companyId, bool? featured, string? sort, int? difficulty, int? category, string? guideLanguage);
    Task<IEnumerable<Tour>> GetFeaturedToursAsync();
    Task<object> GetTourCompaniesAsync();
    Task<IEnumerable<object>> GetTourCategoriesAsync();
    Task<Tour?> GetTourAsync(int id);
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetMyToursAsync(int visitorId);
    Task<(Tour? tour, string? errorMessage, string? errorCode, int? statusCode)> CreateTourAsync(int visitorId, Tour tour);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdateTourAsync(int visitorId, int id, Tour tour);
    Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeleteTourAsync(int visitorId, int id);
    Task<(bool success, object? result, int statusCode)> UploadCoverPhotoAsync(int visitorId, int tourId, Stream fileStream, string fileName);
    Task<(bool success, object? result, int statusCode)> DeleteCoverPhotoAsync(int visitorId, int tourId);
}
