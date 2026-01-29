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

    public ReservationsController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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
