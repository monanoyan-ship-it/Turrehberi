using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ErkanTatilPlani.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
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
        Response.Cookies.Delete("Tours.Jwt");
        Response.Cookies.Delete("ErkanTatilPlani.Jwt");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SetAuthToken([FromBody] SetAuthTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Unauthorized(new { success = false });

        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7078";
        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(HttpMethod.Get, $"{apiBaseUrl}/api/auth/me");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);

        var response = await client.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            return Unauthorized(new { success = false });

        Response.Cookies.Append("Tours.Jwt", request.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });

        return Ok(new { success = true });
    }

    [HttpPost]
    public IActionResult ClearAuthToken()
    {
        Response.Cookies.Delete("Tours.Jwt");
        Response.Cookies.Delete("ErkanTatilPlani.Jwt");
        return Ok(new { success = true });
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

    public IActionResult Notifications()
    {
        return View();
    }

    public IActionResult Messages()
    {
        return View();
    }

    public IActionResult Loyalty()
    {
        return View();
    }

    public IActionResult TravelerClub()
    {
        return View();
    }

    public IActionResult CreateStory()
    {
        return View();
    }

    // GET - Normal gosterim veya redirect sonrasi
    [HttpGet]
    public IActionResult PaymentResult(string? status, int? reservationId, string? error)
    {
        ViewBag.Status = status;
        ViewBag.ReservationId = reservationId;
        ViewBag.Error = error;
        return View();
    }

    // POST - iyzico callback (token ile)
    [HttpPost]
    public async Task<IActionResult> PaymentResult([FromForm] string? token, [FromQuery] int? reservationId)
    {
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Status = "error";
            ViewBag.Error = "Odeme token'i bulunamadi";
            return View();
        }

        try
        {
            // API'ye callback istegi gonder
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7078";
            var client = _httpClientFactory.CreateClient();

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("token", token)
            };

            // reservationId varsa onu da gonder (fallback olarak)
            if (reservationId.HasValue)
            {
                formData.Add(new KeyValuePair<string, string>("reservationId", reservationId.Value.ToString()));
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync($"{apiBaseUrl}/api/reservations/payment/callback", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<PaymentCallbackResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null && result.Success)
            {
                ViewBag.Status = "success";
                ViewBag.ReservationId = result.ReservationId;
            }
            else
            {
                ViewBag.Status = "error";
                ViewBag.ReservationId = result?.ReservationId;
                ViewBag.Error = result?.Message ?? "Odeme basarisiz";
            }
        }
        catch
        {
            ViewBag.Status = "error";
            ViewBag.Error = "Odeme sonucu islenirken bir hata olustu";
            ViewBag.ReservationId = reservationId;
        }

        return View();
    }
}

public class PaymentCallbackResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int ReservationId { get; set; }
    public string? PaymentId { get; set; }
}

public class SetAuthTokenRequest
{
    public string Token { get; set; } = string.Empty;
}
