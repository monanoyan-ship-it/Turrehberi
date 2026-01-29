using ErkanTatilPlani.Core.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ErkanTatilPlani.API.Services;

/// <summary>
/// Dosya yukleme servisi implementasyonu
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _uploadsPath;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _env = env;
        _logger = logger;
        _uploadsPath = Path.Combine(_env.WebRootPath, "uploads");

        // Uploads klasorlerini olustur
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        var folders = new[] { "reviews", "galleries", "avatars", "contracts" };
        foreach (var folder in folders)
        {
            var path = Path.Combine(_uploadsPath, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    public async Task<FileUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder, int maxWidth = 1920)
    {
        try
        {
            // Dosya uzantisi kontrolu
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return FileUploadResult.Fail("Gecersiz dosya formati. Sadece JPG, PNG, GIF ve WebP kabul edilir.");
            }

            // Dosya boyutu kontrolu
            if (fileStream.Length > MaxFileSize)
            {
                return FileUploadResult.Fail("Dosya boyutu 10 MB'dan buyuk olamaz.");
            }

            // Resmi isle
            using var image = await Image.LoadAsync(fileStream);

            // MIME type belirle
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            // Boyutlandirma
            if (image.Width > maxWidth)
            {
                var ratio = (double)maxWidth / image.Width;
                var newHeight = (int)(image.Height * ratio);
                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            // Benzersiz dosya adi olustur
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(_uploadsPath, folder);
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Kaydet
            await image.SaveAsync(filePath, new JpegEncoder { Quality = 85 });

            var fileInfo = new FileInfo(filePath);

            return new FileUploadResult
            {
                Success = true,
                Url = $"/uploads/{folder}/{uniqueFileName}",
                FileName = uniqueFileName,
                FileSize = fileInfo.Length,
                Width = image.Width,
                Height = image.Height,
                MimeType = mimeType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya yukleme hatasi: {FileName}", fileName);
            return FileUploadResult.Fail("Dosya yuklenirken bir hata olustu.");
        }
    }

    public async Task<FileUploadResult> UploadImageWithThumbnailAsync(Stream fileStream, string fileName, string folder, int maxWidth = 1920, int thumbnailWidth = 300)
    {
        try
        {
            // Dosya uzantisi kontrolu
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return FileUploadResult.Fail("Gecersiz dosya formati. Sadece JPG, PNG, GIF ve WebP kabul edilir.");
            }

            // Dosya boyutu kontrolu
            if (fileStream.Length > MaxFileSize)
            {
                return FileUploadResult.Fail("Dosya boyutu 10 MB'dan buyuk olamaz.");
            }

            // Resmi isle
            using var image = await Image.LoadAsync(fileStream);
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // MIME type belirle
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            // Benzersiz dosya adi olustur
            var uniqueId = Guid.NewGuid();
            var uniqueFileName = $"{uniqueId}{extension}";
            var thumbnailFileName = $"{uniqueId}_thumb{extension}";

            var folderPath = Path.Combine(_uploadsPath, folder);
            var filePath = Path.Combine(folderPath, uniqueFileName);
            var thumbnailPath = Path.Combine(folderPath, thumbnailFileName);

            // Ana resim boyutlandirma
            if (image.Width > maxWidth)
            {
                var ratio = (double)maxWidth / image.Width;
                var newHeight = (int)(image.Height * ratio);
                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            // Ana resmi kaydet
            await image.SaveAsync(filePath, new JpegEncoder { Quality = 85 });

            // Thumbnail olustur
            using var thumbImage = await Image.LoadAsync(fileStream.CanSeek ? fileStream : File.OpenRead(filePath));
            var thumbRatio = (double)thumbnailWidth / thumbImage.Width;
            var thumbHeight = (int)(thumbImage.Height * thumbRatio);
            thumbImage.Mutate(x => x.Resize(thumbnailWidth, thumbHeight));
            await thumbImage.SaveAsync(thumbnailPath, new JpegEncoder { Quality = 80 });

            var fileInfo = new FileInfo(filePath);

            return new FileUploadResult
            {
                Success = true,
                Url = $"/uploads/{folder}/{uniqueFileName}",
                ThumbnailUrl = $"/uploads/{folder}/{thumbnailFileName}",
                FileName = uniqueFileName,
                FileSize = fileInfo.Length,
                Width = image.Width,
                Height = image.Height,
                MimeType = mimeType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya yukleme hatasi: {FileName}", fileName);
            return FileUploadResult.Fail("Dosya yuklenirken bir hata olustu.");
        }
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult(false);

            // URL'den dosya yolunu cikar
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                // Thumbnail varsa onu da sil
                var thumbPath = GetThumbnailPath(filePath);
                if (File.Exists(thumbPath))
                {
                    File.Delete(thumbPath);
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya silme hatasi: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }

    public bool FileExists(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return false;

        var relativePath = fileUrl.TrimStart('/');
        var filePath = Path.Combine(_env.WebRootPath, relativePath);
        return File.Exists(filePath);
    }

    private static string GetThumbnailPath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? "";
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        return Path.Combine(directory, $"{fileNameWithoutExt}_thumb{extension}");
    }
}
