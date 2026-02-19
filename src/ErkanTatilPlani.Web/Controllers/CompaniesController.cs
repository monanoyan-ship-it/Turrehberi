using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class CompaniesController : Controller
{
    private readonly IConfiguration _configuration;

    public CompaniesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        return View();
    }

    /// <summary>
    /// Firma profil sayfasi - SEO-friendly URL
    /// /{slug}
    /// </summary>
    [Route("{slug:companySlug}")]
    public IActionResult Details(string slug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.Slug = slug;
        return View();
    }

    /// <summary>
    /// Firma ozel sayfasi
    /// /{slug}/page/{pageSlug}
    /// </summary>
    [Route("{slug:companySlug}/page/{pageSlug}")]
    public IActionResult Page(string slug, string pageSlug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.CompanySlug = slug;
        ViewBag.PageSlug = pageSlug;
        return View();
    }

    /// <summary>
    /// Firma blog listesi
    /// /{slug}/blog
    /// </summary>
    [Route("{slug:companySlug}/blog")]
    public IActionResult Blog(string slug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.CompanySlug = slug;
        return View();
    }

    /// <summary>
    /// Firma blog detay
    /// /{slug}/blog/{postSlug}
    /// </summary>
    [Route("{slug:companySlug}/blog/{postSlug}")]
    public IActionResult BlogDetail(string slug, string postSlug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.CompanySlug = slug;
        ViewBag.PostSlug = postSlug;
        return View();
    }
}
