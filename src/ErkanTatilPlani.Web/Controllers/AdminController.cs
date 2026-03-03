using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class AdminController : Controller
{
    private readonly IConfiguration _configuration;

    public AdminController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private void SetCommonViewData(string activeMenu, string title)
    {
        ViewData["ActiveMenu"] = activeMenu;
        ViewData["Title"] = title;
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.UserRole = "Admin";
    }

    // Dashboard - Tum roller erisebilir
    public IActionResult Index()
    {
        SetCommonViewData("Dashboard", "Kontrol Paneli");
        return View();
    }

    // ============================================
    // STAFF VE ADMIN SAYFALARI
    // ============================================

    // Tum turlar (Staff ve Admin)
    public IActionResult Tours()
    {
        SetCommonViewData("Tours", "Tur Yonetimi");
        return View();
    }

    // Tum firmalar (Staff ve Admin)
    public IActionResult Companies()
    {
        SetCommonViewData("Companies", "Firma Yonetimi");
        return View();
    }

    // Tum kullanicilar (Staff ve Admin)
    public IActionResult Visitors()
    {
        SetCommonViewData("Visitors", "Kullanici Yonetimi");
        return View();
    }

    // Tum rezervasyonlar (Staff ve Admin)
    public IActionResult Reservations()
    {
        SetCommonViewData("Reservations", "Rezervasyon Yonetimi");
        return View();
    }

    // ============================================
    // SADECE ADMIN SAYFALARI
    // ============================================

    // Sistem ayarlari (Sadece Admin)
    public IActionResult Settings()
    {
        SetCommonViewData("Settings", "Sistem Ayarlari");
        return View();
    }

    // Dil yonetimi (Sadece Admin)
    public IActionResult Languages()
    {
        SetCommonViewData("Languages", "Dil Yonetimi");
        return View();
    }

    // Blog yonetimi (Staff ve Admin)
    public IActionResult BlogPosts()
    {
        SetCommonViewData("BlogPosts", "Blog Yonetimi");
        return View();
    }

    // Yorum moderasyonu (Staff ve Admin)
    public IActionResult Reviews()
    {
        SetCommonViewData("Reviews", "Yorum Moderasyonu");
        return View();
    }

    // Email hesaplari yonetimi (Sadece Admin)
    public IActionResult EmailAccounts()
    {
        SetCommonViewData("EmailAccounts", "Email Hesaplari");
        return View();
    }

    // Email sablonlari yonetimi (Sadece Admin)
    public IActionResult EmailTemplates()
    {
        SetCommonViewData("EmailTemplates", "Email Sablonlari");
        return View();
    }

    // SSS yonetimi (Staff ve Admin)
    public IActionResult Faqs()
    {
        SetCommonViewData("Faqs", "SSS Yonetimi");
        return View();
    }

    // Promosyon yonetimi (Staff ve Admin)
    public IActionResult Promotions()
    {
        SetCommonViewData("Promotions", "Promosyon Yonetimi");
        return View();
    }
}
