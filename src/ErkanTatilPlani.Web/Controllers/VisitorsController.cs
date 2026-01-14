using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class VisitorsController : Controller
{
    private readonly IConfiguration _configuration;

    public VisitorsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        return View();
    }
}
