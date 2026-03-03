using System.Net;

namespace ErkanTatilPlani.Tests.Integration;

public class CompaniesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CompaniesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCompanies_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/companies");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanyBySlug_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/companies/slug/nonexistent-company");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
