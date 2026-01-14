using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class ReservationsController : Controller
{
    private readonly IConfiguration _configuration;

    public ReservationsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        return View();
    }
}
