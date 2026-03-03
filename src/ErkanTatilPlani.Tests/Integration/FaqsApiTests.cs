using System.Net;
using System.Net.Http.Json;

namespace ErkanTatilPlani.Tests.Integration;

public class FaqsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FaqsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPublicFaqs_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/faqs/public");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllFaqs_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/faqs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateFaq_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/faqs", new
        {
            Question = "Test soru?",
            Answer = "Test cevap",
            DisplayOrder = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFaqById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/faqs/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
