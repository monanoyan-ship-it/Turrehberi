using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ErkanTatilPlani.Tests.Integration;

public class MarketplaceApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private const int TestCompanyId = 9001;
    private const int TestCompanyOwnerId = 9001;

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MarketplaceApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
        EnsureCompanyOwner(dbContext);
    }

    [Fact]
    public async Task AdminOverview_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/marketplace/admin/overview");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MyFinance_WithVisitorRole_ReturnsForbidden()
    {
        var response = await SendWithTokenAsync("/api/marketplace/my", userId: 8, role: "Visitor");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminOverview_WithAdminToken_ReturnsSummaryAndRecentLists()
    {
        var response = await SendWithTokenAsync("/api/marketplace/admin/overview", userId: 1, role: "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = await ReadJsonAsync(response);
        Assert.True(json.RootElement.TryGetProperty("summary", out _));
        Assert.True(json.RootElement.TryGetProperty("recentTransactions", out _));
        Assert.True(json.RootElement.TryGetProperty("recentPayouts", out _));
    }

    [Theory]
    [InlineData("/api/marketplace/admin/sellers", "sellers")]
    [InlineData("/api/marketplace/admin/transactions", "transactions")]
    [InlineData("/api/marketplace/admin/refunds", "refunds")]
    [InlineData("/api/marketplace/admin/payouts", "payouts")]
    public async Task AdminFinanceLists_WithAdminToken_ReturnExpectedCollection(string url, string propertyName)
    {
        var response = await SendWithTokenAsync(url, userId: 1, role: "Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = await ReadJsonAsync(response);
        Assert.True(json.RootElement.TryGetProperty(propertyName, out var collection));
        Assert.Equal(JsonValueKind.Array, collection.ValueKind);
    }

    [Fact]
    public async Task MyFinance_WithCompanyOwnerToken_ReturnsSellerSummaryAndCollections()
    {
        var response = await SendWithTokenAsync("/api/marketplace/my", TestCompanyOwnerId, role: "CompanyOwner");

        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, content);
        using var json = await ReadJsonAsync(response);
        Assert.True(json.RootElement.TryGetProperty("seller", out _));
        Assert.True(json.RootElement.TryGetProperty("summary", out _));
        Assert.True(json.RootElement.TryGetProperty("transactions", out var transactions));
        Assert.True(json.RootElement.TryGetProperty("payouts", out var payouts));
        Assert.True(json.RootElement.TryGetProperty("refunds", out var refunds));
        Assert.Equal(JsonValueKind.Array, transactions.ValueKind);
        Assert.Equal(JsonValueKind.Array, payouts.ValueKind);
        Assert.Equal(JsonValueKind.Array, refunds.ValueKind);
    }

    private async Task<HttpResponseMessage> SendWithTokenAsync(string url, int userId, string role)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId, role));
        return await _client.SendAsync(request);
    }

    private static void EnsureCompanyOwner(AppDbContext dbContext)
    {
        if (!dbContext.Companies.Any(c => c.Id == TestCompanyId))
        {
            dbContext.Companies.Add(new Company
            {
                Id = TestCompanyId,
                Name = "Marketplace Test Tur",
                Description = "Integration test company",
                Email = "marketplace-test@turrehberi.local",
                Phone = "0212 000 0000",
                Address = "Test Mahallesi No:1",
                Website = "www.marketplace-test.local",
                LogoUrl = "https://example.com/logo.png",
                TaxNumber = "9001000001",
                Slug = "marketplace-test-tur",
                MetaTitle = "Marketplace Test Tur",
                MetaDescription = "Marketplace integration test company",
                Tagline = "Test tours",
                City = "Istanbul",
                CoverImageUrl = "https://example.com/cover.png",
                StatusId = CompanyStatuses.Ids.Approved,
                ApplicationDate = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow,
                SellerLegalTypeId = SellerLegalTypes.Ids.LimitedOrJointStockCompany,
                SellerOnboardingStatusId = SellerOnboardingStatuses.Ids.MissingInfo,
                PlatformCommissionRate = 12,
                PayoutDelayDays = 7,
                IsActive = true
            });
        }

        if (!dbContext.Visitors.Any(v => v.Id == TestCompanyOwnerId))
        {
            dbContext.Visitors.Add(new Visitor
            {
                Id = TestCompanyOwnerId,
                FirstName = "Marketplace",
                LastName = "Owner",
                Email = "marketplace-owner@turrehberi.local",
                Phone = "0532 000 0000",
                IdentityNumber = "90010000010",
                PasswordHash = string.Empty,
                UserTypeId = UserTypes.Ids.CompanyOwner,
                CompanyId = TestCompanyId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        dbContext.SaveChanges();
    }

    private string CreateToken(int userId, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var key = configuration["Jwt:Key"]!;
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
