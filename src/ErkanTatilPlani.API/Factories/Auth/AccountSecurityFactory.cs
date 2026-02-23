using System.Security.Cryptography;
using System.Text;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Auth;
using ErkanTatilPlani.Core.Helpers;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;

namespace ErkanTatilPlani.API.Factories.Auth;

public class AccountSecurityFactory : IAccountSecurityFactory
{
    private readonly IVisitorEntityService _visitorService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public AccountSecurityFactory(
        IVisitorEntityService visitorService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _visitorService = visitorService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<(bool success, object result, int statusCode)> ForgotPasswordAsync(string email, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, new { error = "Validation.EmailRequired" }, 400);

        var visitor = await _visitorService.GetActiveByEmailAsync(email);

        // Guvenlik: Kullanici bulunamasa bile basarili mesaji dondur
        if (visitor == null)
            return (true, new { message = "Success.PasswordResetLinkSent" }, 200);

        // Token olustur (GUID + timestamp)
        var token = Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString();
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // Token'i kaydet (1 saat gecerli)
        visitor.PasswordResetToken = tokenHash;
        visitor.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Sifre sifirlama email'i gonder
        var resetUrl = $"{baseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(visitor.Email)}";
        var customerName = $"{visitor.FirstName} {visitor.LastName}";

        await _emailService.SendPasswordResetEmailAsync(
            visitor.Email,
            resetUrl,
            customerName,
            visitor.PreferredLanguage);

        return (true, new { message = "Success.PasswordResetLinkSent" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, new { error = "Validation.EmailRequired" }, 400);
        if (string.IsNullOrWhiteSpace(token))
            return (false, new { error = "Validation.TokenRequired" }, 400);
        if (string.IsNullOrWhiteSpace(newPassword))
            return (false, new { error = "Validation.NewPasswordRequired" }, 400);
        if (newPassword.Length < 6)
            return (false, new { error = "Validation.PasswordMinLength" }, 400);

        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        var visitor = await _visitorService.GetActiveByEmailAsync(email);
        if (visitor == null ||
            visitor.PasswordResetToken != tokenHash ||
            visitor.PasswordResetTokenExpiry <= DateTime.UtcNow)
        {
            return (false, new { error = "Error.InvalidOrExpiredResetLink" }, 400);
        }

        visitor.PasswordHash = PasswordHelper.HashPassword(newPassword);
        visitor.PasswordResetToken = null;
        visitor.PasswordResetTokenExpiry = null;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Success.PasswordChanged" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> VerifyResetTokenAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return (false, new { valid = false, error = "Validation.EmailTokenRequired" }, 400);

        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        var visitor = await _visitorService.GetActiveByEmailAsync(email);
        var isValid = visitor != null &&
                      visitor.PasswordResetToken == tokenHash &&
                      visitor.PasswordResetTokenExpiry > DateTime.UtcNow;

        return (true, new { valid = isValid }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> SendVerificationEmailAsync(string? baseUrl)
    {
        var visitor = await _currentUserService.GetCurrentVisitorAsync();
        if (visitor == null)
            return (false, new { }, 401);

        if (visitor.EmailVerified)
            return (false, new { error = "Error.EmailAlreadyVerified" }, 400);

        // Token olustur (GUID + timestamp)
        var token = Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString();
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // Token'i kaydet (24 saat gecerli)
        visitor.EmailVerificationToken = tokenHash;
        visitor.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Email dogrulama linki gonder
        var verifyUrl = $"{baseUrl}/Account/VerifyEmail?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(visitor.Email)}";
        var customerName = $"{visitor.FirstName} {visitor.LastName}";

        await _emailService.SendEmailVerificationAsync(
            visitor.Email,
            verifyUrl,
            customerName,
            visitor.PreferredLanguage);

        return (true, new { message = "Success.VerificationEmailSent" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> VerifyEmailAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return (false, new { success = false, error = "Validation.EmailTokenRequired" }, 400);

        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        var visitor = await _visitorService.GetActiveByEmailAsync(email);
        if (visitor == null ||
            visitor.EmailVerificationToken != tokenHash ||
            visitor.EmailVerificationTokenExpiry <= DateTime.UtcNow)
        {
            return (false, new { success = false, error = "Error.InvalidOrExpiredVerificationLink" }, 400);
        }

        // Email dogrulandi
        visitor.EmailVerified = true;
        visitor.EmailVerificationToken = null;
        visitor.EmailVerificationTokenExpiry = null;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { success = true, message = "Success.EmailVerified" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> VerifyEmailTokenAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return (false, new { valid = false, error = "Validation.EmailTokenRequired" }, 400);

        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        var visitor = await _visitorService.GetActiveByEmailAsync(email);
        var isValid = visitor != null &&
                      visitor.EmailVerificationToken == tokenHash &&
                      visitor.EmailVerificationTokenExpiry > DateTime.UtcNow;

        return (true, new { valid = isValid }, 200);
    }
}
