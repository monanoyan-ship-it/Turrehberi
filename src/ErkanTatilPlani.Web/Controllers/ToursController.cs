using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class ToursController : Controller
{
    private readonly IConfiguration _configuration;

    public ToursController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        return View();
    }

    public IActionResult Details(int id)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.TourId = id;
        return View();
    }
}
