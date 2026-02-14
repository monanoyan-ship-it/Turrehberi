namespace ErkanTatilPlani.Core.Factories.EmailAccounts;

public interface IEmailAccountFactory
{
    Task<object> GetAllAsync();
    Task<object?> GetByIdAsync(int id);
    Task<(bool success, object result, int statusCode)> CreateAsync(string name, string? description, string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, string fromEmail, string fromName, bool enableSsl, bool isDefault, int displayOrder);
    Task<(bool success, string message, int statusCode)> UpdateAsync(int id, string name, string? description, string smtpHost, int smtpPort, string smtpUsername, string? smtpPassword, string fromEmail, string fromName, bool enableSsl, int displayOrder);
    Task<(bool success, string message, int statusCode)> DeleteAsync(int id);
    Task<(bool success, string message, int statusCode)> SetDefaultAsync(int id);
    Task<(bool success, object? result, int statusCode)> CopyAsync(int id);
    Task<(bool success, string message, int statusCode)> TestAsync(int id, string toEmail);
}
