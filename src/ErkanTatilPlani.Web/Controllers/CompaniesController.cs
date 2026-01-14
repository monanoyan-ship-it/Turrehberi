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
}
