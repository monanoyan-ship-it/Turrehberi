using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
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
            UserTypeId = visitor.UserTypeId,
            UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Unknown",
            CompanyId = visitor.CompanyId,
            CompanyName = visitor.Company?.Name,
            CompanyStatusId = visitor.Company?.StatusId,
            CompanyStatusName = visitor.Company != null ? CompanyStatuses.GetById(visitor.Company.StatusId)?.SystemName : null,
            PreferredLanguage = visitor.PreferredLanguage
        });
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
