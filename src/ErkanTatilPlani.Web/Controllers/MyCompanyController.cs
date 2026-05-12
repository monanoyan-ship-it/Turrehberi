using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

[Authorize(Roles = "CompanyOwner,Staff,Admin")]
public class MyCompanyController : Controller
{
    private readonly IConfiguration _configuration;

    public MyCompanyController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private void SetCommonViewData(string activeMenu, string title)
    {
        ViewData["ActiveMenu"] = activeMenu;
        ViewData["Title"] = title;
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
    }

    // Firma sahibi dashboard'u
    public IActionResult Index()
    {
        SetCommonViewData("CompanyDashboard", "Dashboard");
        return View("CompanyDashboard");
    }

    // Firma sahibinin kendi turlari
    public IActionResult MyTours()
    {
        SetCommonViewData("MyTours", "Turlarim");
        return View();
    }

    // Firma sahibinin kendi rezervasyonlari
    public IActionResult MyReservations()
    {
        SetCommonViewData("MyReservations", "Rezervasyonlarim");
        return View();
    }

    // Firma sahibinin turlarindaki yorumlar
    public IActionResult MyReviews()
    {
        SetCommonViewData("MyReviews", "Yorumlar");
        return View();
    }

    // Firma sahibinin sayfalari
    public IActionResult CompanyPages()
    {
        SetCommonViewData("CompanyPages", "Sayfalarim");
        return View();
    }

    // Firma sahibinin blogu
    public IActionResult CompanyBlog()
    {
        SetCommonViewData("CompanyBlog", "Blogum");
        return View();
    }

    // Firma promosyonlari
    public IActionResult Promotions()
    {
        SetCommonViewData("Promotions", "Promosyonlarim");
        return View();
    }

    // Firma rehberleri
    public IActionResult Guides()
    {
        SetCommonViewData("Guides", "Rehberlerim");
        return View();
    }

    // Tur takvimi
    public IActionResult TourCalendar()
    {
        SetCommonViewData("TourCalendar", "Tur Takvimi");
        return View();
    }

    // Mesajlar
    public IActionResult Messages()
    {
        SetCommonViewData("Messages", "Mesajlar");
        return View();
    }

    // Analitik
    public IActionResult Analytics()
    {
        SetCommonViewData("Analytics", "Analitik");
        return View();
    }

    // Finans ve hak edisler
    public IActionResult Finance()
    {
        SetCommonViewData("Finance", "Finans");
        return View();
    }
}
