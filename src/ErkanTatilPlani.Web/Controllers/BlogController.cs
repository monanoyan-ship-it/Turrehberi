using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class BlogController : Controller
{
    private readonly IConfiguration _configuration;

    public BlogController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        return View();
    }

    [Route("Blog/{slug}")]
    public IActionResult Detail(string slug)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.Slug = slug;
        return View();
    }
}
