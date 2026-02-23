using ErkanTatilPlani.Core.Services;
using Microsoft.Extensions.Logging;

namespace ErkanTatilPlani.API.Services;

public class ExternalNotificationService : IExternalNotificationService
{
    private readonly ILogger<ExternalNotificationService> _logger;

    public ExternalNotificationService(ILogger<ExternalNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendWhatsAppAsync(string phoneNumber, string message)
    {
        // Stub: Gercek API entegrasyonu sonra eklenecek
        _logger.LogInformation("[WhatsApp] To: {Phone}, Message: {Message}", phoneNumber, message);
        return Task.FromResult(true);
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        // Stub: Gercek API entegrasyonu sonra eklenecek
        _logger.LogInformation("[SMS] To: {Phone}, Message: {Message}", phoneNumber, message);
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailNotificationAsync(string email, string subject, string message)
    {
        // Stub: Gercek email gonderimi sonra eklenecek
        _logger.LogInformation("[Email] To: {Email}, Subject: {Subject}, Message: {Message}", email, subject, message);
        return Task.FromResult(true);
    }
}
