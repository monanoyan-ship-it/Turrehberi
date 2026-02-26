using ErkanTatilPlani.Core.Factories.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthFactory _authFactory;
    private readonly IProfileFactory _profileFactory;
    private readonly IAccountSecurityFactory _accountSecurityFactory;

    public AuthController(
        IAuthFactory authFactory,
        IProfileFactory profileFactory,
        IAccountSecurityFactory accountSecurityFactory)
    {
        _authFactory = authFactory;
        _profileFactory = profileFactory;
        _accountSecurityFactory = accountSecurityFactory;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, result, statusCode) = await _authFactory.LoginAsync(request.Email, request.Password);
        return StatusCode(statusCode, result);
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, result, statusCode) = await _authFactory.RegisterAsync(
            request.FirstName, request.LastName, request.Email, request.Password,
            request.Phone, request.IdentityNumber, request.ReferralCode);
        return StatusCode(statusCode, result);
    }

    [HttpPost("register-company")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyRequest request)
    {
        var (success, result, statusCode) = await _authFactory.RegisterCompanyAsync(
            request.FirstName, request.LastName, request.Email, request.Password, request.Phone,
            request.Company?.Name ?? string.Empty, request.Company?.TaxNumber ?? string.Empty,
            request.Company?.Email, request.Company?.Phone,
            request.Company?.Address, request.Company?.Website, request.Company?.ApplicationNotes);
        return StatusCode(statusCode, result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var (success, result, statusCode) = await _profileFactory.GetCurrentUserAsync();
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var (success, result, statusCode) = await _profileFactory.UpdateProfileAsync(
            request.FirstName, request.LastName, request.Phone, request.Address, request.BirthDate, request.Bio);
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var (success, result, statusCode) = await _profileFactory.ChangePasswordAsync(
            request.CurrentPassword, request.NewPassword);
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    [HttpPut("language")]
    [Authorize]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        var (success, result, statusCode) = await _profileFactory.UpdatePreferredLanguageAsync(request.Language);
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    [HttpPut("notification-preferences")]
    [Authorize]
    public async Task<IActionResult> UpdateNotificationPreferences([FromBody] object preferences)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(preferences);
        var (success, result, statusCode) = await _profileFactory.UpdateNotificationPreferencesAsync(json);
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Sifremi unuttum - email ile sifre sifirlama linki gonder
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("sensitive")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var (success, result, statusCode) = await _accountSecurityFactory.ForgotPasswordAsync(request.Email, request.BaseUrl);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Sifre sifirla - token ile yeni sifre belirle
    /// </summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting("sensitive")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var (success, result, statusCode) = await _accountSecurityFactory.ResetPasswordAsync(
            request.Email, request.Token, request.NewPassword);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Token gecerliligini kontrol et
    /// </summary>
    [HttpGet("verify-reset-token")]
    public async Task<IActionResult> VerifyResetToken([FromQuery] string email, [FromQuery] string token)
    {
        var (success, result, statusCode) = await _accountSecurityFactory.VerifyResetTokenAsync(email, token);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Email dogrulama linki gonder
    /// </summary>
    [HttpPost("send-verification-email")]
    [Authorize]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmailRequest request)
    {
        var (success, result, statusCode) = await _accountSecurityFactory.SendVerificationEmailAsync(request.BaseUrl);
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Email adresini dogrula
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var (success, result, statusCode) = await _accountSecurityFactory.VerifyEmailAsync(email, token);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Email dogrulama token gecerliligini kontrol et
    /// </summary>
    [HttpGet("verify-email-token")]
    public async Task<IActionResult> VerifyEmailToken([FromQuery] string email, [FromQuery] string token)
    {
        var (success, result, statusCode) = await _accountSecurityFactory.VerifyEmailTokenAsync(email, token);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Profil resmi yukle
    /// </summary>
    [HttpPost("avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Error.NoFileSelected" });

        using var stream = file.OpenReadStream();
        var (success, result, statusCode) = await _profileFactory.UploadAvatarAsync(
            file.FileName, file.ContentType, file.Length, stream);
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Profil resmini sil
    /// </summary>
    [HttpDelete("avatar")]
    [Authorize]
    public async Task<IActionResult> DeleteAvatar()
    {
        var (success, result, statusCode) = await _profileFactory.DeleteAvatarAsync();
        if (!success && statusCode == 401)
            return Unauthorized();
        return StatusCode(statusCode, result);
    }
}

// DTOs
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdentityNumber { get; set; }
    public string? ReferralCode { get; set; }
}

public class RegisterCompanyRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public CompanyInfo Company { get; set; } = null!;
}

public class CompanyInfo
{
    public string Name { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? ApplicationNotes { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = null!;
}

public class UserInfo
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? AvatarUrl { get; set; }
    public bool EmailVerified { get; set; }
    public int UserTypeId { get; set; }
    public string UserTypeName { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? CompanyStatusId { get; set; }
    public string? CompanyStatusName { get; set; }
    public string PreferredLanguage { get; set; } = "tr";
    public string? NotificationPreference { get; set; }

    // Gezgin Kulubu - Sosyal Profil
    public string? Bio { get; set; }
    public bool IsProfilePublic { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int TripStoryCount { get; set; }
}

public class UpdateLanguageRequest
{
    public string Language { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Bio { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class SendVerificationEmailRequest
{
    public string? BaseUrl { get; set; }
}
