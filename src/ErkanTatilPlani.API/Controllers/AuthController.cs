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
                CompanyName = visitor.Company?.Name
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
                UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Visitor"
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
            CompanyName = visitor.Company?.Name
        });
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
}
