using System.Net;
using System.Net.Http.Json;

namespace ErkanTatilPlani.Tests.Integration;

public class SupportApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SupportApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSupportTicket_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/support", new
        {
            Name = "Test User",
            Email = "test@test.com",
            Message = "Test mesaj",
            Subject = "Test"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetSupportTickets_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/support");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSupportStats_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/support/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
