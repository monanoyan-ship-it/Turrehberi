using System.Net;

namespace ErkanTatilPlani.Tests.Integration;

public class LocalizationApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LocalizationApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLanguages_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/localization/languages");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLocale_Turkish_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/localization/tr");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLocale_English_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/localization/en");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
