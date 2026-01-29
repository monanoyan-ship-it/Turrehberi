using ErkanTatilPlani.Core.Enums;
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
        // TODO: Gercek auth sistemi kuruldugunda session'dan alinacak
        ViewBag.UserRole = "Admin"; // Admin, Staff, CompanyOwner
    }

    // Dashboard - Tum roller erisebilir
    public IActionResult Index()
    {
        SetCommonViewData("Dashboard", "Kontrol Paneli");
        return View();
    }

    // ============================================
    // FIRMA SAHIBI SAYFALARI
    // ============================================

    // Firma sahibi dashboard'u
    public IActionResult CompanyDashboard()
    {
        SetCommonViewData("CompanyDashboard", "Dashboard");
        ViewBag.UserRole = "CompanyOwner";
        return View();
    }

    // Firma sahibinin kendi turlari
    public IActionResult MyTours()
    {
        SetCommonViewData("MyTours", "Turlarim");
        ViewBag.UserRole = "CompanyOwner";
        return View();
    }

    // Firma sahibinin kendi rezervasyonlari
    public IActionResult MyReservations()
    {
        SetCommonViewData("MyReservations", "Rezervasyonlarim");
        ViewBag.UserRole = "CompanyOwner";
        return View();
    }

    // Firma sahibinin turlarindaki yorumlar
    public IActionResult MyReviews()
    {
        SetCommonViewData("MyReviews", "Yorumlar");
        ViewBag.UserRole = "CompanyOwner";
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
}
