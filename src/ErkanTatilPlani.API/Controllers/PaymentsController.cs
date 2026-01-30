using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Services;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        AppDbContext context,
        IPaymentService paymentService,
        IEmailService emailService,
        ILogger<PaymentsController> logger)
    {
        _context = context;
        _paymentService = paymentService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Rezervasyon icin odeme baslat
    /// </summary>
    [HttpPost("initialize/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> InitializePayment(int reservationId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.VisitorId == visitorId && r.IsActive);

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        // Zaten tam odenmis mi kontrol et
        if (reservation.PaymentStatus == PaymentStatusEnum.FullyPaid)
            return BadRequest(new { message = "Bu rezervasyon zaten odenmis" });

        // Rezervasyon durumu Pending veya Confirmed olmali
        if (reservation.Status == ReservationStatus.Cancelled)
            return BadRequest(new { message = "Iptal edilmis rezervasyon icin odeme yapilamaz" });

        // Callback URL'i olustur
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/payments/callback";

        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            Amount = reservation.TotalPrice,
            CustomerEmail = reservation.Visitor.Email,
            CustomerName = reservation.Visitor.FirstName,
            CustomerSurname = reservation.Visitor.LastName,
            CustomerPhone = reservation.Visitor.Phone ?? "",
            CustomerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            CustomerAddress = reservation.Visitor.Address ?? "",
            ProductName = reservation.Tour.Name,
            ProductCategory = "Tur",
            CallbackUrl = callbackUrl
        };

        var result = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (result.Success)
        {
            // Token'i rezervasyona kaydet
            reservation.PaymentToken = result.Token;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                paymentPageUrl = result.PaymentPageUrl,
                token = result.Token
            });
        }
        else
        {
            return BadRequest(new
            {
                success = false,
                message = result.ErrorMessage,
                errorCode = result.ErrorCode
            });
        }
    }

    /// <summary>
    /// Kalan odemeyi baslat (on odeme yapildiktan sonra)
    /// </summary>
    [HttpPost("initialize-remaining/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> InitializeRemainingPayment(int reservationId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.VisitorId == visitorId && r.IsActive);

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        // On odeme yapilmis mi kontrol et
        if (reservation.PaymentStatus != PaymentStatusEnum.DepositPaid)
            return BadRequest(new { message = "Bu islem icin once on odeme yapilmis olmalidir" });

        // Kalan tutari hesapla
        var remainingAmount = reservation.TotalPrice - reservation.PaidAmount;
        if (remainingAmount <= 0)
            return BadRequest(new { message = "Odenecek tutar kalmadi" });

        // Callback URL'i olustur
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/payments/callback";

        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            Amount = remainingAmount,
            CustomerEmail = reservation.Visitor.Email,
            CustomerName = reservation.Visitor.FirstName,
            CustomerSurname = reservation.Visitor.LastName,
            CustomerPhone = reservation.Visitor.Phone ?? "",
            CustomerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            CustomerAddress = reservation.Visitor.Address ?? "",
            ProductName = $"{reservation.Tour.Name} - Kalan Odeme",
            ProductCategory = "Tur",
            CallbackUrl = callbackUrl
        };

        var result = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (result.Success)
        {
            reservation.PaymentToken = result.Token;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                paymentPageUrl = result.PaymentPageUrl,
                token = result.Token,
                remainingAmount
            });
        }
        else
        {
            return BadRequest(new
            {
                success = false,
                message = result.ErrorMessage,
                errorCode = result.ErrorCode
            });
        }
    }

    /// <summary>
    /// Iyzico callback endpoint'i
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromForm] string token)
    {
        _logger.LogInformation("Payment callback received for token: {Token}", token);

        var result = await _paymentService.ProcessCallbackAsync(token);

        if (result.Success)
        {
            // Rezervasyonu bul ve guncelle
            var reservation = await _context.Reservations
                .Include(r => r.Tour)
                    .ThenInclude(t => t.Company)
                .Include(r => r.Visitor)
                .FirstOrDefaultAsync(r => r.Id == result.ReservationId);

            if (reservation != null)
            {
                // Odenen tutari guncelle
                reservation.PaidAmount += result.PaidAmount ?? 0;
                reservation.PaymentId = result.PaymentId;
                reservation.PaidAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                // Tam odeme mi on odeme mi kontrol et
                if (reservation.PaidAmount >= reservation.TotalPrice)
                {
                    reservation.PaymentStatus = PaymentStatusEnum.FullyPaid;
                }
                else
                {
                    reservation.PaymentStatus = PaymentStatusEnum.DepositPaid;
                }

                reservation.Status = ReservationStatus.Confirmed; // Otomatik onayla
                await _context.SaveChangesAsync();

                // Basarili odeme emaili gonder
                var emailModel = new ReservationEmailModel
                {
                    ToEmail = reservation.Visitor.Email,
                    CustomerName = $"{reservation.Visitor.FirstName} {reservation.Visitor.LastName}",
                    TourName = reservation.Tour.Name,
                    CompanyName = reservation.Tour.Company.Name,
                    Destination = reservation.Tour.Destination,
                    StartDate = reservation.StartDate,
                    EndDate = reservation.EndDate,
                    NumberOfPeople = reservation.NumberOfPeople,
                    TotalPrice = reservation.TotalPrice,
                    PreferredLanguage = reservation.Visitor.PreferredLanguage ?? "tr"
                };
                await _emailService.SendReservationConfirmedEmailAsync(emailModel);

                _logger.LogInformation("Payment successful for reservation {ReservationId}", result.ReservationId);

                // Basarili sayfasina yonlendir
                return Redirect($"/Account/PaymentResult?status=success&reservationId={result.ReservationId}");
            }
        }
        else
        {
            // Basarisiz odeme
            if (result.ReservationId > 0)
            {
                var reservation = await _context.Reservations.FindAsync(result.ReservationId);
                if (reservation != null)
                {
                    reservation.PaymentStatus = PaymentStatusEnum.Failed;
                    reservation.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogWarning("Payment failed for reservation {ReservationId}: {ErrorMessage}",
                result.ReservationId, result.ErrorMessage);

            // Basarisiz sayfasina yonlendir
            return Redirect($"/Account/PaymentResult?status=failed&reservationId={result.ReservationId}&error={Uri.EscapeDataString(result.ErrorMessage ?? "Odeme basarisiz")}");
        }

        return Redirect("/Account/Reservations");
    }

    /// <summary>
    /// Odeme durumunu sorgula
    /// </summary>
    [HttpGet("status/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetPaymentStatus(int reservationId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservation = await _context.Reservations
            .Where(r => r.Id == reservationId && r.VisitorId == visitorId && r.IsActive)
            .Select(r => new
            {
                r.Id,
                r.PaymentId,
                PaymentStatus = r.PaymentStatus.ToString(),
                PaymentStatusId = (int)r.PaymentStatus,
                r.PaidAt,
                r.TotalPrice
            })
            .FirstOrDefaultAsync();

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        return Ok(reservation);
    }

    /// <summary>
    /// Bekleyen odemesi olan rezervasyonlari listele
    /// </summary>
    [HttpGet("pending")]
    [Authorize]
    public async Task<ActionResult<object>> GetPendingPayments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservations = await _context.Reservations
            .Include(r => r.Tour)
            .Where(r => r.VisitorId == visitorId &&
                        r.IsActive &&
                        r.PaymentStatus == PaymentStatusEnum.Pending &&
                        r.Status != ReservationStatus.Cancelled)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                r.TotalPrice,
                r.StartDate,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(new { reservations });
    }
}
