using System.Net;

namespace ErkanTatilPlani.Tests.Integration;

public class ToursApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ToursApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTours_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tours");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFeaturedTours_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tours/featured");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTourById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/tours/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
