using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Tours;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Tours;

public class TourPhotoFactory : ITourPhotoFactory
{
    private readonly ITourEntityService _tourService;
    private readonly ITourPhotoEntityService _photoService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUnitOfWork _unitOfWork;

    public TourPhotoFactory(
        ITourEntityService tourService,
        ITourPhotoEntityService photoService,
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork)
    {
        _tourService = tourService;
        _photoService = photoService;
        _fileUploadService = fileUploadService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object? result, int statusCode)> UploadPhotoAsync(int tourId, Stream fileStream, string fileName, string? title, bool isCover)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null)
            return (false, new { message = "Error.TourNotFound" }, 404);

        // Maksimum 20 fotograf
        var photoCount = await _photoService.GetPhotoCountAsync(tourId);
        if (photoCount >= 20)
            return (false, new { message = "Error.MaxTourPhotosReached" }, 400);

        // Dosya kontrolu
        if (fileStream == null || fileStream.Length == 0)
            return (false, new { message = "Error.NoFileSelected" }, 400);

        // Yukleme
        var result = await _fileUploadService.UploadImageWithThumbnailAsync(fileStream, fileName, "tours");

        if (!result.Success)
            return (false, new { message = result.ErrorMessage }, 400);

        // Eger isCover ise diger cover fotograflarin durumunu guncelle
        if (isCover)
        {
            await _photoService.ClearCoverAsync(tourId);
        }

        // Veritabanina kaydet
        var photo = new TourPhoto
        {
            TourId = tourId,
            ImageUrl = result.Url!,
            ThumbnailUrl = result.ThumbnailUrl ?? result.Url!,
            Title = title ?? string.Empty,
            DisplayOrder = photoCount + 1,
            IsCover = isCover,
            FileSize = result.FileSize,
            Width = result.Width,
            Height = result.Height,
            MimeType = result.MimeType ?? "image/jpeg"
        };

        _photoService.Add(photo);
        await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            message = "Success.PhotoUploaded",
            photo = new
            {
                photo.Id,
                photo.ImageUrl,
                photo.ThumbnailUrl,
                photo.Title,
                photo.DisplayOrder,
                photo.IsCover
            }
        }, 200);
    }

    public async Task<(bool found, object? result)> GetPhotosAsync(int tourId)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null)
            return (false, null);

        var photos = await _photoService.GetActivePhotos(tourId)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id,
                p.ImageUrl,
                p.ThumbnailUrl,
                p.Title,
                p.DisplayOrder,
                p.Width,
                p.Height,
                p.IsCover
            })
            .ToListAsync();

        return (true, photos);
    }

    public async Task<(bool success, object? result, int statusCode)> DeletePhotoAsync(int tourId, int photoId)
    {
        var photo = await _photoService.GetByIdAsync(photoId, tourId);
        if (photo == null)
            return (false, new { message = "Error.PhotoNotFound" }, 404);

        // Dosyalari sil
        await _fileUploadService.DeleteFileAsync(photo.ImageUrl);
        if (!string.IsNullOrEmpty(photo.ThumbnailUrl))
            await _fileUploadService.DeleteFileAsync(photo.ThumbnailUrl);

        // Soft delete
        photo.IsActive = false;
        photo.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.PhotoDeleted" }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> SetCoverPhotoAsync(int tourId, int photoId)
    {
        var photo = await _photoService.GetByIdAsync(photoId, tourId);
        if (photo == null || !photo.IsActive)
            return (false, new { message = "Error.PhotoNotFound" }, 404);

        await _photoService.ClearCoverExceptAsync(tourId, photoId);
        photo.IsCover = true;
        photo.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.CoverPhotoUpdated" }, 200);
    }

    public async Task<(bool success, object? result, int statusCode)> ReorderPhotosAsync(int tourId, List<int> photoIds)
    {
        if (photoIds == null || !photoIds.Any())
            return (false, new { message = "Validation.PhotoListRequired" }, 400);

        var photos = await _photoService.GetByIdsAsync(tourId, photoIds);

        for (int i = 0; i < photoIds.Count; i++)
        {
            var photo = photos.FirstOrDefault(p => p.Id == photoIds[i]);
            if (photo != null)
            {
                photo.DisplayOrder = i + 1;
                photo.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.OrderUpdated" }, 200);
    }
}
