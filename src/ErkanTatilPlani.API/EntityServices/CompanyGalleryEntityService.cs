using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class CompanyGalleryEntityService : ICompanyGalleryEntityService
{
    private readonly AppDbContext _context;

    public CompanyGalleryEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetImageCountAsync(int companyId)
        => await _context.CompanyGalleryImages.CountAsync(g => g.CompanyId == companyId && g.IsActive);

    public async Task<CompanyGalleryImage?> GetByIdAsync(int imageId, int companyId)
        => await _context.CompanyGalleryImages.FirstOrDefaultAsync(g => g.Id == imageId && g.CompanyId == companyId);

    public IQueryable<CompanyGalleryImage> GetActiveImages(int companyId)
        => _context.CompanyGalleryImages.Where(g => g.CompanyId == companyId && g.IsActive);

    public async Task<List<CompanyGalleryImage>> GetByIdsAsync(int companyId, List<int> imageIds)
        => await _context.CompanyGalleryImages.Where(g => g.CompanyId == companyId && imageIds.Contains(g.Id) && g.IsActive).ToListAsync();

    public void Add(CompanyGalleryImage image) => _context.CompanyGalleryImages.Add(image);

    public void Update(CompanyGalleryImage image) => _context.Entry(image).State = EntityState.Modified;

    public async Task ClearFeaturedAsync(int companyId)
        => await _context.CompanyGalleryImages.Where(g => g.CompanyId == companyId && g.IsFeatured).ExecuteUpdateAsync(s => s.SetProperty(g => g.IsFeatured, false));

    public async Task ClearFeaturedExceptAsync(int companyId, int imageId)
        => await _context.CompanyGalleryImages.Where(g => g.CompanyId == companyId && g.IsFeatured && g.Id != imageId).ExecuteUpdateAsync(s => s.SetProperty(g => g.IsFeatured, false));
}
