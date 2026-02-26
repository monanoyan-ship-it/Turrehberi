namespace ErkanTatilPlani.Core.Factories.Auth;

public interface IProfileFactory
{
    Task<(bool success, object? result, int statusCode)> GetCurrentUserAsync();

    Task<(bool success, object? result, int statusCode)> UpdateProfileAsync(
        string firstName, string lastName, string? phone, string? address, DateTime? birthDate, string? bio);

    Task<(bool success, object result, int statusCode)> ChangePasswordAsync(string currentPassword, string newPassword);

    Task<(bool success, object result, int statusCode)> UpdatePreferredLanguageAsync(string language);

    Task<(bool success, object result, int statusCode)> UploadAvatarAsync(
        string fileName, string contentType, long fileLength, Stream fileStream);

    Task<(bool success, object result, int statusCode)> DeleteAvatarAsync();
    Task<(bool success, object result, int statusCode)> UpdateNotificationPreferencesAsync(string preferencesJson);
}
