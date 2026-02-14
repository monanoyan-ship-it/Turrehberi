namespace ErkanTatilPlani.Core.Factories.Companies;

public interface ICompanyGalleryFactory
{
    Task<(bool success, object? result, int statusCode)> UploadGalleryImageAsync(int companyId, Stream fileStream, string fileName, string? title, string? description, bool isFeatured);
    Task<(bool found, object? result)> GetGalleryImagesAsync(int companyId);
    Task<(bool success, object? result, int statusCode)> UpdateGalleryImageAsync(int companyId, int imageId, string? title, string? description, string? altText, int? displayOrder, bool? isFeatured);
    Task<(bool success, object? result, int statusCode)> DeleteGalleryImageAsync(int companyId, int imageId);
    Task<(bool success, object? result, int statusCode)> ReorderGalleryImagesAsync(int companyId, List<int> imageIds);
}
