namespace ErkanTatilPlani.Core.Services;

/// <summary>
/// Dosya yukleme servisi interface
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Resim yukle ve URL dondur
    /// </summary>
    Task<FileUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder, int maxWidth = 1920);

    /// <summary>
    /// Resim yukle ve thumbnail olustur
    /// </summary>
    Task<FileUploadResult> UploadImageWithThumbnailAsync(Stream fileStream, string fileName, string folder, int maxWidth = 1920, int thumbnailWidth = 300);

    /// <summary>
    /// Dosya sil
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Dosya var mi kontrol et
    /// </summary>
    bool FileExists(string fileUrl);
}

/// <summary>
/// Dosya yukleme sonucu
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? MimeType { get; set; }
    public string? ErrorMessage { get; set; }

    public static FileUploadResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
