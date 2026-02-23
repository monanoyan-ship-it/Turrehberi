namespace ErkanTatilPlani.Core.Services;

public interface IExternalNotificationService
{
    Task<bool> SendWhatsAppAsync(string phoneNumber, string message);
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendEmailNotificationAsync(string email, string subject, string message);
}
