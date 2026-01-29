using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace ErkanTatilPlani.Web.Controllers;

public class SitemapController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public SitemapController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [Route("sitemap.xml")]
    [ResponseCache(Duration = 3600)] // 1 hour cache
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7078";

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urlset = new XElement(ns + "urlset");

        // Static pages
        var staticPages = new[]
        {
            new { Loc = "/", Priority = "1.0", ChangeFreq = "daily" },
            new { Loc = "/Tours", Priority = "0.9", ChangeFreq = "daily" },
            new { Loc = "/Companies", Priority = "0.8", ChangeFreq = "weekly" },
            new { Loc = "/Account/Login", Priority = "0.5", ChangeFreq = "monthly" },
            new { Loc = "/Account/Register", Priority = "0.5", ChangeFreq = "monthly" }
        };

        foreach (var page in staticPages)
        {
            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", baseUrl + page.Loc),
                new XElement(ns + "changefreq", page.ChangeFreq),
                new XElement(ns + "priority", page.Priority)
            ));
        }

        // Fetch companies from API
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var companiesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/companies/public");

            if (companiesResponse.IsSuccessStatusCode)
            {
                var companiesJson = await companiesResponse.Content.ReadFromJsonAsync<CompaniesApiResponse>();
                if (companiesJson?.Companies != null)
                {
                    foreach (var company in companiesJson.Companies)
                    {
                        if (!string.IsNullOrEmpty(company.Slug))
                        {
                            urlset.Add(new XElement(ns + "url",
                                new XElement(ns + "loc", $"{baseUrl}/Firmalar/{company.Slug}"),
                                new XElement(ns + "changefreq", "weekly"),
                                new XElement(ns + "priority", "0.7")
                            ));
                        }
                    }
                }
            }
        }
        catch
        {
            // Silently continue if API is unavailable
        }

        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            urlset
        );

        return Content(document.ToString(), "application/xml", Encoding.UTF8);
    }

    // DTO for API response
    private class CompaniesApiResponse
    {
        public List<CompanyDto>? Companies { get; set; }
    }

    private class CompanyDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
    }
}
