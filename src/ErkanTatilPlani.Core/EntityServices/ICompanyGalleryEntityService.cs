using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface ICompanyGalleryEntityService
{
    Task<int> GetImageCountAsync(int companyId);
    Task<CompanyGalleryImage?> GetByIdAsync(int imageId, int companyId);
    IQueryable<CompanyGalleryImage> GetActiveImages(int companyId);
    Task<List<CompanyGalleryImage>> GetByIdsAsync(int companyId, List<int> imageIds);
    void Add(CompanyGalleryImage image);
    void Update(CompanyGalleryImage image);
    Task ClearFeaturedAsync(int companyId);
    Task ClearFeaturedExceptAsync(int companyId, int imageId);
}
