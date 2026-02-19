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
    /// /Companies/Details/{slug}
    /// </summary>
    [Route("Companies/Details/{slug}")]
    public IActionResult Details(string slug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.Slug = slug;
        return View();
    }

    /// <summary>
    /// Firma ozel sayfasi
    /// /Companies/{slug}/page/{pageSlug}
    /// </summary>
    [Route("Companies/{slug}/page/{pageSlug}")]
    public IActionResult Page(string slug, string pageSlug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.CompanySlug = slug;
        ViewBag.PageSlug = pageSlug;
        return View();
    }

    /// <summary>
    /// Firma blog listesi
    /// /Companies/{slug}/blog
    /// </summary>
    [Route("Companies/{slug}/blog")]
    public IActionResult Blog(string slug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.CompanySlug = slug;
        return View();
    }

    /// <summary>
    /// Firma blog detay
    /// /Companies/{slug}/blog/{postSlug}
    /// </summary>
    [Route("Companies/{slug}/blog/{postSlug}")]
    public IActionResult BlogDetail(string slug, string postSlug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.CompanySlug = slug;
        ViewBag.PostSlug = postSlug;
        return View();
    }
}
