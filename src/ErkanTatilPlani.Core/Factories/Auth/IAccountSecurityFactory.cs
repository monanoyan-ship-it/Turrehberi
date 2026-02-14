namespace ErkanTatilPlani.Core.Factories.Auth;

public interface IAccountSecurityFactory
{
    Task<(bool success, object result, int statusCode)> ForgotPasswordAsync(string email, string? baseUrl);

    Task<(bool success, object result, int statusCode)> ResetPasswordAsync(string email, string token, string newPassword);

    Task<(bool success, object result, int statusCode)> VerifyResetTokenAsync(string email, string token);

    Task<(bool success, object result, int statusCode)> SendVerificationEmailAsync(string? baseUrl);

    Task<(bool success, object result, int statusCode)> VerifyEmailAsync(string email, string token);

    Task<(bool success, object result, int statusCode)> VerifyEmailTokenAsync(string email, string token);
}
