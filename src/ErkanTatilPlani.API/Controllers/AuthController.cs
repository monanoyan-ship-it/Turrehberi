using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(AppDbContext context, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _context = context;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "Email ve sifre zorunludur" });

        var visitor = await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Email == request.Email && v.IsActive);

        if (visitor == null)
            return Unauthorized(new { error = "Email veya sifre hatali" });

        if (!VerifyPassword(request.Password, visitor.PasswordHash))
            return Unauthorized(new { error = "Email veya sifre hatali" });

        var token = GenerateJwtToken(visitor);

        return Ok(new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Id = visitor.Id,
                FirstName = visitor.FirstName,
                LastName = visitor.LastName,
                Email = visitor.Email,
                UserTypeId = visitor.UserTypeId,
                UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Unknown",
                CompanyId = visitor.CompanyId,
                CompanyName = visitor.Company?.Name,
                CompanyStatusId = visitor.Company?.StatusId,
                CompanyStatusName = visitor.Company != null ? CompanyStatuses.GetById(visitor.Company.StatusId)?.SystemName : null,
                PreferredLanguage = visitor.PreferredLanguage
            }
        });
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "Email ve sifre zorunludur" });

        if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            return BadRequest(new { error = "Ad ve soyad zorunludur" });

        if (request.Password.Length < 6)
            return BadRequest(new { error = "Sifre en az 6 karakter olmalidir" });

        // Check if email exists
        var existingUser = await _context.Visitors.FirstOrDefaultAsync(v => v.Email == request.Email);
        if (existingUser != null)
            return BadRequest(new { error = "Bu email adresi zaten kayitli" });

        // Create new visitor
        var visitor = new Visitor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone ?? string.Empty,
            IdentityNumber = request.IdentityNumber ?? string.Empty,
            PasswordHash = HashPassword(request.Password),
            UserTypeId = UserTypes.Ids.Visitor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Visitors.Add(visitor);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(visitor);

        return Ok(new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Id = visitor.Id,
                FirstName = visitor.FirstName,
                LastName = visitor.LastName,
                Email = visitor.Email,
                UserTypeId = visitor.UserTypeId,
                UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Visitor",
                PreferredLanguage = visitor.PreferredLanguage
            }
        });
    }

    [HttpPost("register-company")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyRequest request)
    {
        // Validation
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "Email ve sifre zorunludur" });

        if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            return BadRequest(new { error = "Ad ve soyad zorunludur" });

        if (request.Password.Length < 6)
            return BadRequest(new { error = "Sifre en az 6 karakter olmalidir" });

        if (request.Company == null || string.IsNullOrEmpty(request.Company.Name))
            return BadRequest(new { error = "Firma adi zorunludur" });

        if (string.IsNullOrEmpty(request.Company.TaxNumber))
            return BadRequest(new { error = "Vergi numarasi zorunludur" });

        // Check if email exists
        var existingUser = await _context.Visitors.FirstOrDefaultAsync(v => v.Email == request.Email);
        if (existingUser != null)
            return BadRequest(new { error = "Bu email adresi zaten kayitli" });

        // Check if tax number exists
        var existingCompany = await _context.Companies.FirstOrDefaultAsync(c => c.TaxNumber == request.Company.TaxNumber);
        if (existingCompany != null)
            return BadRequest(new { error = "Bu vergi numarasi zaten kayitli" });

        // Create company (Pending - onay bekliyor)
        var company = new Company
        {
            Name = request.Company.Name,
            TaxNumber = request.Company.TaxNumber,
            Email = request.Company.Email ?? string.Empty,
            Phone = request.Company.Phone ?? string.Empty,
            Address = request.Company.Address ?? string.Empty,
            Website = request.Company.Website ?? string.Empty,
            Description = string.Empty,
            LogoUrl = string.Empty,
            // Basvuru durumu
            StatusId = CompanyStatuses.Ids.Pending,
            ApplicationDate = DateTime.UtcNow,
            ApplicationNotes = request.Company.ApplicationNotes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Create visitor as company owner
        var visitor = new Visitor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone ?? string.Empty,
            IdentityNumber = string.Empty,
            PasswordHash = HashPassword(request.Password),
            UserTypeId = UserTypes.Ids.CompanyOwner,
            CompanyId = company.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Visitors.Add(visitor);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(visitor);

        return Ok(new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Id = visitor.Id,
                FirstName = visitor.FirstName,
                LastName = visitor.LastName,
                Email = visitor.Email,
                UserTypeId = visitor.UserTypeId,
                UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "CompanyOwner",
                CompanyId = visitor.CompanyId,
                CompanyName = company.Name,
                CompanyStatusId = company.StatusId,
                CompanyStatusName = CompanyStatuses.GetById(company.StatusId)?.SystemName ?? "Pending",
                PreferredLanguage = visitor.PreferredLanguage
            }
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);

        if (visitor == null)
            return Unauthorized();

        return Ok(new UserInfo
        {
            Id = visitor.Id,
            FirstName = visitor.FirstName,
            LastName = visitor.LastName,
            Email = visitor.Email,
            Phone = visitor.Phone,
            Address = visitor.Address,
            BirthDate = visitor.BirthDate,
            AvatarUrl = visitor.AvatarUrl,
            EmailVerified = visitor.EmailVerified,
            UserTypeId = visitor.UserTypeId,
            UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Unknown",
            CompanyId = visitor.CompanyId,
            CompanyName = visitor.Company?.Name,
            CompanyStatusId = visitor.Company?.StatusId,
            CompanyStatusName = visitor.Company != null ? CompanyStatuses.GetById(visitor.Company.StatusId)?.SystemName : null,
            PreferredLanguage = visitor.PreferredLanguage
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
        if (visitor == null)
            return Unauthorized();

        // Validation
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(new { error = "Ad zorunludur" });
        if (string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { error = "Soyad zorunludur" });

        // Update fields
        visitor.FirstName = request.FirstName.Trim();
        visitor.LastName = request.LastName.Trim();
        visitor.Phone = request.Phone?.Trim() ?? string.Empty;
        visitor.Address = request.Address?.Trim();
        visitor.BirthDate = request.BirthDate;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new UserInfo
        {
            Id = visitor.Id,
            FirstName = visitor.FirstName,
            LastName = visitor.LastName,
            Email = visitor.Email,
            Phone = visitor.Phone,
            Address = visitor.Address,
            BirthDate = visitor.BirthDate,
            AvatarUrl = visitor.AvatarUrl,
            EmailVerified = visitor.EmailVerified,
            UserTypeId = visitor.UserTypeId,
            UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Unknown",
            CompanyId = visitor.CompanyId,
            CompanyName = visitor.Company?.Name,
            CompanyStatusId = visitor.Company?.StatusId,
            CompanyStatusName = visitor.Company != null ? CompanyStatuses.GetById(visitor.Company.StatusId)?.SystemName : null,
            PreferredLanguage = visitor.PreferredLanguage
        });
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
        if (visitor == null)
            return Unauthorized();

        // Validation
        if (string.IsNullOrEmpty(request.CurrentPassword))
            return BadRequest(new { error = "Mevcut sifre zorunludur" });
        if (string.IsNullOrEmpty(request.NewPassword))
            return BadRequest(new { error = "Yeni sifre zorunludur" });
        if (request.NewPassword.Length < 6)
            return BadRequest(new { error = "Yeni sifre en az 6 karakter olmalidir" });

        // Verify current password
        if (!VerifyPassword(request.CurrentPassword, visitor.PasswordHash))
            return BadRequest(new { error = "Mevcut sifre hatali" });

        // Update password
        visitor.PasswordHash = HashPassword(request.NewPassword);
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Sifre basariyla degistirildi" });
    }

    [HttpPut("language")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
        if (visitor == null)
            return Unauthorized();

        // Validate language code
        var validLanguages = new[] { "tr", "en", "ru", "de", "es", "fr", "ar", "fa", "pt" };
        if (!validLanguages.Contains(request.Language))
            return BadRequest(new { error = "Gecersiz dil kodu" });

        visitor.PreferredLanguage = request.Language;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { language = visitor.PreferredLanguage });
    }

    /// <summary>
    /// Sifremi unuttum - email ile sifre sifirlama linki gonder
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("sensitive")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email zorunludur" });

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Email == request.Email && v.IsActive);

        // Guvenlik: Kullanici bulunamasa bile basarili mesaji dondur
        if (visitor == null)
            return Ok(new { message = "Eger bu email adresi sistemde kayitliysa, sifre sifirlama linki gonderildi." });

        // Token olustur (GUID + timestamp)
        var token = Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString();
        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(token)));

        // Token'i kaydet (1 saat gecerli)
        visitor.PasswordResetToken = tokenHash;
        visitor.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Gercek uygulamada burada email gonderilir
        // Simdilik token'i response'ta dondur (development icin)
        var resetUrl = $"{request.BaseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(visitor.Email)}";

        // TODO: Email gonderme servisi eklendiginde burada email gonderilecek
        // await _emailService.SendPasswordResetEmail(visitor.Email, resetUrl);

        return Ok(new
        {
            message = "Eger bu email adresi sistemde kayitliysa, sifre sifirlama linki gonderildi.",
            // Development icin - production'da kaldirilacak
            debug = new
            {
                resetUrl,
                token,
                expiresAt = visitor.PasswordResetTokenExpiry
            }
        });
    }

    /// <summary>
    /// Sifre sifirla - token ile yeni sifre belirle
    /// </summary>
    [HttpPost("reset-password")]
    [EnableRateLimiting("sensitive")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email zorunludur" });
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "Token zorunludur" });
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { error = "Yeni sifre zorunludur" });
        if (request.NewPassword.Length < 6)
            return BadRequest(new { error = "Sifre en az 6 karakter olmalidir" });

        // Token hash'le
        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(request.Token)));

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v =>
            v.Email == request.Email &&
            v.IsActive &&
            v.PasswordResetToken == tokenHash &&
            v.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (visitor == null)
            return BadRequest(new { error = "Gecersiz veya suresi dolmus sifre sifirlama linki" });

        // Sifreyi guncelle
        visitor.PasswordHash = HashPassword(request.NewPassword);
        visitor.PasswordResetToken = null;
        visitor.PasswordResetTokenExpiry = null;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Sifreniz basariyla degistirildi. Simdi giris yapabilirsiniz." });
    }

    /// <summary>
    /// Token gecerliligini kontrol et
    /// </summary>
    [HttpGet("verify-reset-token")]
    public async Task<IActionResult> VerifyResetToken([FromQuery] string email, [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return BadRequest(new { valid = false, error = "Email ve token zorunludur" });

        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(token)));

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v =>
            v.Email == email &&
            v.IsActive &&
            v.PasswordResetToken == tokenHash &&
            v.PasswordResetTokenExpiry > DateTime.UtcNow);

        return Ok(new { valid = visitor != null });
    }

    /// <summary>
    /// Email dogrulama linki gonder
    /// </summary>
    [HttpPost("send-verification-email")]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmailRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
        if (visitor == null)
            return Unauthorized();

        if (visitor.EmailVerified)
            return BadRequest(new { error = "Email adresi zaten dogrulanmis" });

        // Token olustur (GUID + timestamp)
        var token = Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString();
        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(token)));

        // Token'i kaydet (24 saat gecerli)
        visitor.EmailVerificationToken = tokenHash;
        visitor.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Gercek uygulamada burada email gonderilir
        var verifyUrl = $"{request.BaseUrl}/Account/VerifyEmail?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(visitor.Email)}";

        // TODO: Email gonderme servisi eklendiginde burada email gonderilecek
        // await _emailService.SendVerificationEmail(visitor.Email, verifyUrl);

        return Ok(new
        {
            message = "Dogrulama linki email adresinize gonderildi.",
            // Development icin - production'da kaldirilacak
            debug = new
            {
                verifyUrl,
                token,
                expiresAt = visitor.EmailVerificationTokenExpiry
            }
        });
    }

    /// <summary>
    /// Email adresini dogrula
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return BadRequest(new { success = false, error = "Email ve token zorunludur" });

        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(token)));

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v =>
            v.Email == email &&
            v.IsActive &&
            v.EmailVerificationToken == tokenHash &&
            v.EmailVerificationTokenExpiry > DateTime.UtcNow);

        if (visitor == null)
            return BadRequest(new { success = false, error = "Gecersiz veya suresi dolmus dogrulama linki" });

        // Email dogrulandi
        visitor.EmailVerified = true;
        visitor.EmailVerificationToken = null;
        visitor.EmailVerificationTokenExpiry = null;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Email adresiniz basariyla dogrulandi." });
    }

    /// <summary>
    /// Email dogrulama token gecerliligini kontrol et
    /// </summary>
    [HttpGet("verify-email-token")]
    public async Task<IActionResult> VerifyEmailToken([FromQuery] string email, [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return BadRequest(new { valid = false, error = "Email ve token zorunludur" });

        var tokenHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(token)));

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v =>
            v.Email == email &&
            v.IsActive &&
            v.EmailVerificationToken == tokenHash &&
            v.EmailVerificationTokenExpiry > DateTime.UtcNow);

        return Ok(new { valid = visitor != null });
    }

    /// <summary>
    /// Profil resmi yukle
    /// </summary>
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
        if (visitor == null)
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Dosya secilmedi" });

        // Dosya boyutu kontrolu (max 2MB)
        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { error = "Dosya boyutu 2MB'i gecemez" });

        // Dosya tipi kontrolu
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { error = "Sadece resim dosyalari yuklenebilir (jpg, png, gif, webp)" });

        // Upload klasorunu olustur
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Eski avatar'i sil
        if (!string.IsNullOrEmpty(visitor.AvatarUrl) && visitor.AvatarUrl.StartsWith("/uploads/avatars/"))
        {
            var oldFilePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", visitor.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);
        }

        // Yeni dosya adi olustur
        var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Dosyayi kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // URL'i kaydet
        visitor.AvatarUrl = $"/uploads/avatars/{fileName}";
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { avatarUrl = visitor.AvatarUrl, message = "Profil resmi basariyla yuklendi" });
    }

    /// <summary>
    /// Profil resmini sil
    /// </summary>
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
        if (visitor == null)
            return Unauthorized();

        // Avatar yoksa
        if (string.IsNullOrEmpty(visitor.AvatarUrl))
            return BadRequest(new { error = "Silinecek profil resmi bulunamadi" });

        // Dosyayi sil
        if (visitor.AvatarUrl.StartsWith("/uploads/avatars/"))
        {
            var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", visitor.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        // URL'i temizle
        visitor.AvatarUrl = null;
        visitor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Profil resmi basariyla silindi" });
    }

    private string GenerateJwtToken(Visitor visitor)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, visitor.Id.ToString()),
            new Claim(ClaimTypes.Email, visitor.Email),
            new Claim(ClaimTypes.Name, $"{visitor.FirstName} {visitor.LastName}"),
            new Claim("UserTypeId", visitor.UserTypeId.ToString()),
            new Claim(ClaimTypes.Role, UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Visitor")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
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
