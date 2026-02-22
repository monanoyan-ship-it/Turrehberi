namespace ErkanTatilPlani.Core.Factories.Tours;

public interface ITourPhotoFactory
{
    Task<(bool success, object? result, int statusCode)> UploadPhotoAsync(int tourId, Stream fileStream, string fileName, string? title, bool isCover);
    Task<(bool found, object? result)> GetPhotosAsync(int tourId);
    Task<(bool success, object? result, int statusCode)> DeletePhotoAsync(int tourId, int photoId);
    Task<(bool success, object? result, int statusCode)> SetCoverPhotoAsync(int tourId, int photoId);
    Task<(bool success, object? result, int statusCode)> ReorderPhotosAsync(int tourId, List<int> photoIds);
}
