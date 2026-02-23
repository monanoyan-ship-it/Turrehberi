using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace ErkanTatilPlani.API.Infrastructure;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Visitor visitor)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

        var claimList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, visitor.Id.ToString()),
            new Claim(ClaimTypes.Email, visitor.Email),
            new Claim(ClaimTypes.Name, $"{visitor.FirstName} {visitor.LastName}"),
            new Claim("UserTypeId", visitor.UserTypeId.ToString()),
            new Claim(ClaimTypes.Role, UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Visitor")
        };

        if (visitor.CompanyId.HasValue)
        {
            claimList.Add(new Claim("CompanyId", visitor.CompanyId.Value.ToString()));
        }

        var claims = claimList.ToArray();

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
