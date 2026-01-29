using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.Web.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Logout()
    {
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Profile()
    {
        return View();
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    public IActionResult ResetPassword(string? token, string? email)
    {
        ViewBag.Token = token;
        ViewBag.Email = email;
        return View();
    }

    public IActionResult VerifyEmail(string? token, string? email)
    {
        ViewBag.Token = token;
        ViewBag.Email = email;
        return View();
    }

    public IActionResult Reservations()
    {
        return View();
    }

    public IActionResult ReservationDetail(int id)
    {
        ViewBag.ReservationId = id;
        return View();
    }

    public IActionResult Favorites()
    {
        return View();
    }

    public IActionResult PaymentResult(string? status, int? reservationId, string? error)
    {
        ViewBag.Status = status;
        ViewBag.ReservationId = reservationId;
        ViewBag.Error = error;
        return View();
    }
}
