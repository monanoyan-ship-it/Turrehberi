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
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public ReservationsController(
        AppDbContext context,
        IEmailService emailService,
        IPaymentService paymentService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _paymentService = paymentService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
    {
        return await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Firma sahibinin kendi turlarına yapilan rezervasyonlari listele
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyReservations([FromQuery] string? status = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitor = await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == int.Parse(userIdClaim) && v.IsActive);

        if (visitor == null)
            return Unauthorized(new { message = "Kullanici bulunamadi" });

        if (visitor.Company == null)
            return StatusCode(403, new { message = "Firma sahibi degilsiniz", code = "NOT_COMPANY_OWNER" });

        // Firmanin turlarinin ID'leri
        var companyTourIds = await _context.Tours
            .Where(t => t.CompanyId == visitor.Company.Id)
            .Select(t => t.Id)
            .ToListAsync();

        var query = _context.Reservations
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .Where(r => companyTourIds.Contains(r.TourId) && r.IsActive);

        // Durum filtresi
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            if (Enum.TryParse<ReservationStatus>(status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }
        }

        var reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                TourId = r.TourId,
                TourName = r.Tour.Name,
                TourDestination = r.Tour.Destination,
                VisitorId = r.VisitorId,
                VisitorName = r.Visitor.FirstName + " " + r.Visitor.LastName,
                VisitorEmail = r.Visitor.Email,
                VisitorPhone = r.Visitor.Phone,
                r.StartDate,
                r.EndDate,
                r.NumberOfPeople,
                r.TotalPrice,
                Status = r.Status.ToString(),
                StatusId = (int)r.Status,
                r.Notes,
                r.CreatedAt
            })
            .ToListAsync();

        // Istatistikler
        var allReservations = await _context.Reservations
            .Where(r => companyTourIds.Contains(r.TourId) && r.IsActive)
            .ToListAsync();

        var stats = new
        {
            total = allReservations.Count,
            pending = allReservations.Count(r => r.Status == ReservationStatus.Pending),
            confirmed = allReservations.Count(r => r.Status == ReservationStatus.Confirmed),
            cancelled = allReservations.Count(r => r.Status == ReservationStatus.Cancelled),
            completed = allReservations.Count(r => r.Status == ReservationStatus.Completed)
        };

        return Ok(new { reservations, stats });
    }

    /// <summary>
    /// Firma sahibi rezervasyon durumunu degistir
    /// </summary>
    [HttpPatch("my/{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateMyReservationStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitor = await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == int.Parse(userIdClaim) && v.IsActive);

        if (visitor?.Company == null)
            return StatusCode(403, new { message = "Firma sahibi degilsiniz" });

        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        // Rezervasyonun firmaya ait oldugunu kontrol et
        if (reservation.Tour.CompanyId != visitor.Company.Id)
            return StatusCode(403, new { message = "Bu rezervasyonu duzenleme yetkiniz yok" });

        if (!Enum.TryParse<ReservationStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { message = "Gecersiz durum" });

        var oldStatus = reservation.Status;
        reservation.Status = newStatus;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Email bildirimi gonder
        if (oldStatus != newStatus)
        {
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
                Notes = reservation.Notes,
                RejectionReason = request.RejectionReason,
                PreferredLanguage = reservation.Visitor.PreferredLanguage ?? "tr"
            };

            if (newStatus == ReservationStatus.Confirmed)
            {
                await _emailService.SendReservationConfirmedEmailAsync(emailModel);
            }
            else if (newStatus == ReservationStatus.Cancelled)
            {
                await _emailService.SendReservationCancelledEmailAsync(emailModel);
            }
        }

        return Ok(new { message = "Rezervasyon durumu guncellendi", status = newStatus.ToString() });
    }

    /// <summary>
    /// Ziyaretcinin kendi rezervasyonlarini listele
    /// </summary>
    [HttpGet("visitor/my")]
    [Authorize]
    public async Task<ActionResult<object>> GetVisitorReservations()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservations = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Where(r => r.VisitorId == visitorId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                TourId = r.TourId,
                TourName = r.Tour.Name,
                TourDestination = r.Tour.Destination,
                TourImageUrl = r.Tour.ImageUrl,
                CompanyName = r.Tour.Company.Name,
                CompanySlug = r.Tour.Company.Slug,
                r.StartDate,
                r.EndDate,
                r.NumberOfPeople,
                r.TotalPrice,
                Status = r.Status.ToString(),
                StatusId = (int)r.Status,
                r.Notes,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(new { reservations });
    }

    /// <summary>
    /// Ziyaretcinin kendi rezervasyon detayini getir
    /// </summary>
    [HttpGet("visitor/my/{id}")]
    [Authorize]
    public async Task<ActionResult<object>> GetVisitorReservationDetail(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Where(r => r.Id == id && r.VisitorId == visitorId && r.IsActive)
            .Select(r => new
            {
                r.Id,
                TourId = r.TourId,
                TourName = r.Tour.Name,
                TourDescription = r.Tour.Description,
                TourDestination = r.Tour.Destination,
                TourImageUrl = r.Tour.ImageUrl,
                TourPrice = r.Tour.Price,
                TourDurationDays = r.Tour.DurationDays,
                CompanyId = r.Tour.Company.Id,
                CompanyName = r.Tour.Company.Name,
                CompanySlug = r.Tour.Company.Slug,
                CompanyPhone = r.Tour.Company.Phone,
                CompanyEmail = r.Tour.Company.Email,
                r.StartDate,
                r.EndDate,
                r.NumberOfPeople,
                r.TotalPrice,
                Status = r.Status.ToString(),
                StatusId = (int)r.Status,
                r.Notes,
                r.CreatedAt,
                r.UpdatedAt,
                CanCancel = r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed,
                // Odeme bilgileri
                r.DepositAmount,
                r.PaidAmount,
                r.PaymentId,
                PaymentStatus = r.PaymentStatus.ToString(),
                PaymentStatusId = (int)r.PaymentStatus,
                r.PaidAt
            })
            .FirstOrDefaultAsync();

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        return Ok(reservation);
    }

    /// <summary>
    /// Ziyaretci kendi rezervasyonunu iptal et
    /// </summary>
    [HttpPut("visitor/my/{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelVisitorReservation(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var visitorId = int.Parse(userIdClaim);

        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == id && r.VisitorId == visitorId && r.IsActive);

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        // Sadece Pending veya Confirmed durumundaki rezervasyonlar iptal edilebilir
        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Confirmed)
            return BadRequest(new { message = "Bu rezervasyon iptal edilemez" });

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Email bildirimi gonder
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
        await _emailService.SendReservationCancelledEmailAsync(emailModel);

        return Ok(new { message = "Rezervasyon iptal edildi" });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return NotFound();
        return reservation;
    }

    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    // ===============================================
    // PUBLIC REZERVASYON VE ODEME ISLEMLERI
    // ===============================================

    /// <summary>
    /// Public rezervasyon olustur ve odeme baslat
    /// </summary>
    [HttpPost("public/create")]
    public async Task<ActionResult<object>> CreatePublicReservation([FromBody] CreateReservationRequest request)
    {
        // Tur kontrolu
        var tour = await _context.Tours
            .Include(t => t.Company)
            .FirstOrDefaultAsync(t => t.Id == request.TourId && t.IsActive);

        if (tour == null)
            return NotFound(new { message = "Tur bulunamadi" });

        if (tour.Company == null || tour.Company.StatusId != Core.Enums.CompanyStatuses.Ids.Approved)
            return BadRequest(new { message = "Bu tur su anda rezervasyona kapali" });

        // Validasyon
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "Ad soyad zorunludur" });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "E-posta zorunludur" });
        if (string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest(new { message = "Telefon zorunludur" });
        if (request.NumberOfPeople < 1)
            return BadRequest(new { message = "Kisi sayisi en az 1 olmalidir" });

        // Giris yapmis kullanici mi kontrol et
        int? visitorId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim))
        {
            visitorId = int.Parse(userIdClaim);
        }
        else
        {
            // Misafir kullanici - email ile mevcut kullanici var mi kontrol et
            var existingVisitor = await _context.Visitors
                .FirstOrDefaultAsync(v => v.Email == request.Email && v.IsActive);

            if (existingVisitor != null)
            {
                visitorId = existingVisitor.Id;
            }
            else
            {
                // Yeni misafir kullanici olustur
                var nameParts = request.FullName.Trim().Split(' ', 2);
                var newVisitor = new Visitor
                {
                    FirstName = nameParts[0],
                    LastName = nameParts.Length > 1 ? nameParts[1] : "",
                    Email = request.Email,
                    Phone = request.Phone,
                    PasswordHash = "", // Misafir kullanici, sifre yok
                    UserTypeId = Core.Enums.UserTypes.Ids.Visitor
                };
                _context.Visitors.Add(newVisitor);
                await _context.SaveChangesAsync();
                visitorId = newVisitor.Id;
            }
        }

        // Toplam fiyat hesapla
        var totalPrice = tour.Price * request.NumberOfPeople;

        // On odeme tutarini hesapla (firmanin ayarlari)
        var depositPercentage = tour.Company.DepositPercentage;
        var depositAmount = totalPrice * depositPercentage / 100;

        // Baslangic ve bitis tarihi (simdilik varsayilan)
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(7);
        var endDate = startDate.AddDays(tour.DurationDays);

        // Rezervasyon olustur
        var reservation = new Reservation
        {
            TourId = request.TourId,
            VisitorId = visitorId!.Value,
            StartDate = startDate,
            EndDate = endDate,
            NumberOfPeople = request.NumberOfPeople,
            TotalPrice = totalPrice,
            DepositAmount = depositAmount,
            PaidAmount = 0,
            Status = ReservationStatus.Pending,
            PaymentStatus = PaymentStatusEnum.Pending,
            Notes = request.Notes ?? ""
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Odeme baslatma
        var webBaseUrl = _configuration["WebBaseUrl"] ?? "https://localhost:7080";
        var callbackUrl = $"{webBaseUrl}/Account/PaymentResult?reservationId={reservation.Id}";

        var nameParts2 = request.FullName.Trim().Split(' ', 2);
        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            Amount = depositAmount,
            CustomerEmail = request.Email,
            CustomerName = nameParts2[0],
            CustomerSurname = nameParts2.Length > 1 ? nameParts2[1] : "",
            CustomerPhone = request.Phone,
            CustomerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            CustomerAddress = request.Address ?? "Belirtilmedi",
            ProductName = $"{tour.Name} - {tour.Destination} ({request.NumberOfPeople} kisi)",
            ProductCategory = "Tur Rezervasyonu",
            CallbackUrl = callbackUrl
        };

        var paymentResult = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (!paymentResult.Success)
        {
            // Odeme baslatma basarisiz, rezervasyonu sil
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = "Odeme baslatilirken bir hata olustu",
                error = paymentResult.ErrorMessage
            });
        }

        // Payment token'i kaydet
        reservation.PaymentToken = paymentResult.Token;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            reservationId = reservation.Id,
            paymentPageUrl = paymentResult.PaymentPageUrl,
            totalPrice,
            depositAmount,
            depositPercentage
        });
    }

    /// <summary>
    /// Odeme callback - iyzico'dan gelen sonucu isle
    /// </summary>
    [HttpPost("payment/callback")]
    public async Task<ActionResult<object>> PaymentCallback([FromForm] string token, [FromForm] int? reservationId = null)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token gerekli" });

        var paymentResult = await _paymentService.ProcessCallbackAsync(token);

        Reservation? reservation = null;

        // 1. Oncelikle ConversationId'den gelen reservationId'yi dene
        if (paymentResult.ReservationId > 0)
        {
            reservation = await _context.Reservations
                .Include(r => r.Tour)
                    .ThenInclude(t => t.Company)
                .Include(r => r.Visitor)
                .FirstOrDefaultAsync(r => r.Id == paymentResult.ReservationId);
        }

        // 2. ConversationId'den bulunamadiysa PaymentToken ile ara
        if (reservation == null)
        {
            reservation = await _context.Reservations
                .Include(r => r.Tour)
                    .ThenInclude(t => t.Company)
                .Include(r => r.Visitor)
                .FirstOrDefaultAsync(r => r.PaymentToken == token);
        }

        // 3. Son cari - form'dan gelen reservationId ile ara
        if (reservation == null && reservationId.HasValue && reservationId.Value > 0)
        {
            reservation = await _context.Reservations
                .Include(r => r.Tour)
                    .ThenInclude(t => t.Company)
                .Include(r => r.Visitor)
                .FirstOrDefaultAsync(r => r.Id == reservationId.Value);
        }

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi", token = token?.Substring(0, Math.Min(20, token?.Length ?? 0)) });

        if (paymentResult.Success)
        {
            // Odeme basarili - odenen tutari guncelle
            reservation.PaidAmount += paymentResult.PaidAmount ?? 0;
            reservation.PaymentId = paymentResult.PaymentId;
            reservation.PaidAt = DateTime.UtcNow;
            reservation.Status = ReservationStatus.Confirmed;

            // Tam odeme mi on odeme mi kontrol et
            if (reservation.PaidAmount >= reservation.TotalPrice)
            {
                reservation.PaymentStatus = PaymentStatusEnum.FullyPaid;
            }
            else
            {
                reservation.PaymentStatus = PaymentStatusEnum.DepositPaid;
            }

            await _context.SaveChangesAsync();

            // Onay emaili gonder
            try
            {
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
                    Notes = reservation.Notes,
                    PreferredLanguage = reservation.Visitor.PreferredLanguage ?? "tr"
                };
                await _emailService.SendReservationConfirmedEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                // Email hatasi rezervasyonu etkilemesin
                Console.WriteLine($"Email gonderme hatasi: {ex.Message}");
            }

            return Ok(new
            {
                success = true,
                message = "Odeme basarili",
                reservationId = reservation.Id,
                paymentId = paymentResult.PaymentId
            });
        }
        else
        {
            // Odeme basarisiz
            reservation.PaymentStatus = PaymentStatusEnum.Failed;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = false,
                message = paymentResult.ErrorMessage ?? "Odeme basarisiz",
                reservationId = reservation.Id
            });
        }
    }

    /// <summary>
    /// Odeme durumunu sorgula
    /// </summary>
    [HttpGet("payment/status/{reservationId}")]
    public async Task<ActionResult<object>> GetPaymentStatus(int reservationId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Tour)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            return NotFound(new { message = "Rezervasyon bulunamadi" });

        return Ok(new
        {
            reservationId = reservation.Id,
            paymentStatus = reservation.PaymentStatus.ToString(),
            reservationStatus = reservation.Status.ToString(),
            totalPrice = reservation.TotalPrice,
            paidAt = reservation.PaidAt,
            tourName = reservation.Tour.Name
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
    {
        if (id != reservation.Id) return BadRequest();
        reservation.UpdatedAt = DateTime.UtcNow;
        _context.Entry(reservation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();
        reservation.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
}

public class CreateReservationRequest
{
    public int TourId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; } = 1;
    public string? Notes { get; set; }
    public string? Address { get; set; }
    public DateTime? StartDate { get; set; }
}
