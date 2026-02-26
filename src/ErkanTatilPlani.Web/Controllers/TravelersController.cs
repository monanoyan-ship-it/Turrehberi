using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class TravelersController : Controller
{
    public IActionResult Profile(int id)
    {
        ViewBag.TravelerId = id;
        return View();
    }

    public IActionResult Story(int id)
    {
        ViewBag.StoryId = id;
        return View();
    }
}
