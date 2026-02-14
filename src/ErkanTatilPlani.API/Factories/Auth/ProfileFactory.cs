using ErkanTatilPlani.Core.Factories.Auth;
using ErkanTatilPlani.Core.Helpers;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace ErkanTatilPlani.API.Factories.Auth;

public class ProfileFactory : IProfileFactory
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public ProfileFactory(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment environment)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<(bool success, object? result, int statusCode)> GetCurrentUserAsync()
    {
        var visitor = await _currentUserService.GetCurrentVisitorWithCompanyAsync();
        if (visitor == null)
            return (false, null, 401);

        return (true, AuthUserInfoHelper.BuildUserInfo(visitor, includeProfileFields: true), 200);
    }

    public async Task<(bool success, object? result, int statusCode)> UpdateProfileAsync(
        string firstName, string lastName, string? phone, string? address, DateTime? birthDate)
    {
        var visitor = await _currentUserService.GetCurrentVisitorWithCompanyAsync();
        if (visitor == null)
            return (false, null, 401);

        if (string.IsNullOrWhiteSpace(firstName))
            return (false, new { error = "Ad zorunludur" }, 400);
        if (string.IsNullOrWhiteSpace(lastName))
            return (false, new { error = "Soyad zorunludur" }, 400);

        visitor.FirstName = firstName.Trim();
        visitor.LastName = lastName.Trim();
        visitor.Phone = phone?.Trim() ?? string.Empty;
        visitor.Address = address?.Trim();
        visitor.BirthDate = birthDate;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, AuthUserInfoHelper.BuildUserInfo(visitor, includeProfileFields: true), 200);
    }

    public async Task<(bool success, object result, int statusCode)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var visitor = await _currentUserService.GetCurrentVisitorAsync();
        if (visitor == null)
            return (false, new { }, 401);

        if (string.IsNullOrEmpty(currentPassword))
            return (false, new { error = "Mevcut sifre zorunludur" }, 400);
        if (string.IsNullOrEmpty(newPassword))
            return (false, new { error = "Yeni sifre zorunludur" }, 400);
        if (newPassword.Length < 6)
            return (false, new { error = "Yeni sifre en az 6 karakter olmalidir" }, 400);

        if (!PasswordHelper.VerifyPassword(currentPassword, visitor.PasswordHash))
            return (false, new { error = "Mevcut sifre hatali" }, 400);

        visitor.PasswordHash = PasswordHelper.HashPassword(newPassword);
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Sifre basariyla degistirildi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> UpdatePreferredLanguageAsync(string language)
    {
        var visitor = await _currentUserService.GetCurrentVisitorAsync();
        if (visitor == null)
            return (false, new { }, 401);

        var validLanguages = new[] { "tr", "en", "ru", "de", "es", "fr", "ar", "fa", "pt" };
        if (!validLanguages.Contains(language))
            return (false, new { error = "Gecersiz dil kodu" }, 400);

        visitor.PreferredLanguage = language;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { language = visitor.PreferredLanguage }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> UploadAvatarAsync(
        string fileName, string contentType, long fileLength, Stream fileStream)
    {
        var visitor = await _currentUserService.GetCurrentVisitorAsync();
        if (visitor == null)
            return (false, new { }, 401);

        if (fileLength == 0)
            return (false, new { error = "Dosya secilmedi" }, 400);

        // Dosya boyutu kontrolu (max 2MB)
        if (fileLength > 2 * 1024 * 1024)
            return (false, new { error = "Dosya boyutu 2MB'i gecemez" }, 400);

        // Dosya tipi kontrolu
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return (false, new { error = "Sadece resim dosyalari yuklenebilir (jpg, png, gif, webp)" }, 400);

        // Upload klasorunu olustur
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Eski avatar'i sil
        if (!string.IsNullOrEmpty(visitor.AvatarUrl) && visitor.AvatarUrl.StartsWith("/uploads/avatars/"))
        {
            var oldFilePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", visitor.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(oldFilePath))
                File.Delete(oldFilePath);
        }

        // Yeni dosya adi olustur
        var newFileName = $"{visitor.Id}_{DateTime.UtcNow.Ticks}{extension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        // Dosyayi kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // URL'i kaydet
        visitor.AvatarUrl = $"/uploads/avatars/{newFileName}";
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { avatarUrl = visitor.AvatarUrl, message = "Profil resmi basariyla yuklendi" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeleteAvatarAsync()
    {
        var visitor = await _currentUserService.GetCurrentVisitorAsync();
        if (visitor == null)
            return (false, new { }, 401);

        if (string.IsNullOrEmpty(visitor.AvatarUrl))
            return (false, new { error = "Silinecek profil resmi bulunamadi" }, 400);

        // Dosyayi sil
        if (visitor.AvatarUrl.StartsWith("/uploads/avatars/"))
        {
            var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", visitor.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        // URL'i temizle
        visitor.AvatarUrl = null;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Profil resmi basariyla silindi" }, 200);
    }
}
