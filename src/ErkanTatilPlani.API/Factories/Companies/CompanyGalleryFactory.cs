using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Companies;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Companies;

public class CompanyGalleryFactory : ICompanyGalleryFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly ICompanyGalleryEntityService _galleryService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyGalleryFactory(
        ICompanyEntityService companyService,
        ICompanyGalleryEntityService galleryService,
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork)
    {
        _companyService = companyService;
        _galleryService = galleryService;
        _fileUploadService = fileUploadService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object? result, int statusCode)> UploadGalleryImageAsync(int companyId, Stream fileStream, string fileName, string? title, string? description, bool isFeatured)
    {
        var company = await _companyService.GetByIdAsync(companyId);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        // Maksimum 20 resim
        var imageCount = await _galleryService.GetImageCountAsync(companyId);
        if (imageCount >= 20)
            return (false, new { message = "Error.GalleryMaxImagesReached" }, 400);

        // Dosya kontrolu
        if (fileStream == null || fileStream.Length == 0)
            return (false, new { message = "Error.NoFileSelected" }, 400);

        // Yukleme
        var result = await _fileUploadService.UploadImageWithThumbnailAsync(fileStream, fileName, "galleries");

        if (!result.Success)
            return (false, new { message = result.ErrorMessage }, 400);

        // Eger isFeatured ise diger featured resimlerin durumunu guncelle
        if (isFeatured)
        {
            await _galleryService.ClearFeaturedAsync(companyId);
        }

        // Veritabanina kaydet
        var galleryImage = new CompanyGalleryImage
        {
            CompanyId = companyId,
            ImageUrl = result.Url!,
            ThumbnailUrl = result.ThumbnailUrl ?? result.Url!,
            Title = title ?? string.Empty,
            Description = description ?? string.Empty,
            DisplayOrder = imageCount + 1,
            FileSize = result.FileSize,
            Width = result.Width,
            Height = result.Height,
            MimeType = result.MimeType ?? "image/jpeg",
            AltText = title ?? $"Galeri resmi {imageCount + 1}",
            IsFeatured = isFeatured
        };

        _galleryService.Add(galleryImage);
        await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            message = "Success.ImageUploaded",
            image = new
            {
                galleryImage.Id,
                galleryImage.ImageUrl,
                galleryImage.ThumbnailUrl,
                galleryImage.Title,
                galleryImage.Description,
                galleryImage.DisplayOrder,
                galleryImage.IsFeatured
            }
        }, 200);
    }

    public async Task<(bool found, object? result)> GetGalleryImagesAsync(int companyId)
    {
        var company = await _companyService.GetByIdAsync(companyId);
        if (company == null)
            return (false, null);

        var images = await _galleryService.GetActiveImages(companyId)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new
            {
                g.Id,
                g.ImageUrl,
                g.ThumbnailUrl,
                g.Title,
                g.Description,
                g.DisplayOrder,
                g.Width,
                g.Height,
                g.IsFeatured
            })
            .ToListAsync();

        return (true, images);
    }

    public async Task<(bool success, object? result, int statusCode)> UpdateGalleryImageAsync(int companyId, int imageId, string? title, string? description, string? altText, int? displayOrder, bool? isFeatured)
    {
        var image = await _galleryService.GetByIdAsync(imageId, companyId);
        if (image == null)
            return (false, new { message = "Error.ImageNotFound" }, 404);

        if (title != null) image.Title = title;
        if (description != null) image.Description = description;
        if (altText != null) image.AltText = altText;
        if (displayOrder.HasValue) image.DisplayOrder = displayOrder.Value;

        if (isFeatured.HasValue && isFeatured.Value)
        {
            // Diger featured'lari kaldir
            await _galleryService.ClearFeaturedExceptAsync(companyId, imageId);
            image.IsFeatured = true;
        }
        else if (isFeatured.HasValue)
        {
            image.IsFeatured = false;
        }

        image.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.ImageUpdated" }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> DeleteGalleryImageAsync(int companyId, int imageId)
    {
        var image = await _galleryService.GetByIdAsync(imageId, companyId);
        if (image == null)
            return (false, new { message = "Error.ImageNotFound" }, 404);

        // Dosyalari sil
        await _fileUploadService.DeleteFileAsync(image.ImageUrl);
        if (!string.IsNullOrEmpty(image.ThumbnailUrl))
            await _fileUploadService.DeleteFileAsync(image.ThumbnailUrl);

        // Veritabanindan sil (soft delete)
        image.IsActive = false;
        image.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.ImageDeleted" }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> ReorderGalleryImagesAsync(int companyId, List<int> imageIds)
    {
        if (imageIds == null || !imageIds.Any())
            return (false, new { message = "Validation.ImageListEmpty" }, 400);

        var images = await _galleryService.GetByIdsAsync(companyId, imageIds);

        for (int i = 0; i < imageIds.Count; i++)
        {
            var image = images.FirstOrDefault(img => img.Id == imageIds[i]);
            if (image != null)
            {
                image.DisplayOrder = i + 1;
                image.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.OrderUpdated" }, 200);
    }
}
