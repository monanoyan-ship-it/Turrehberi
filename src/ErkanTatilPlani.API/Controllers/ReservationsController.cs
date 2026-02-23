using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationCrudFactory _crud;
    private readonly IVisitorReservationFactory _visitor;
    private readonly IReservationPaymentFactory _payment;

    public ReservationsController(
        IReservationCrudFactory crud,
        IVisitorReservationFactory visitor,
        IReservationPaymentFactory payment)
    {
        _crud = crud;
        _visitor = visitor;
        _payment = payment;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
    {
        return Ok(await _crud.GetAllAsync());
    }

    /// <summary>
    /// Firma sahibinin kendi turlarına yapilan rezervasyonlari listele
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<object>> GetMyReservations([FromQuery] string? status = null)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (result, errorMessage, errorCode, statusCode) = await _visitor.GetMyReservationsAsync(visitorId.Value, status);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    /// <summary>
    /// Firma sahibi rezervasyon durumunu degistir
    /// </summary>
    [HttpPatch("my/{id}/status")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdateMyReservationStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _visitor.UpdateMyReservationStatusAsync(visitorId.Value, id, request.Status, request.RejectionReason);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Ziyaretcinin kendi rezervasyonlarini listele
    /// </summary>
    [HttpGet("visitor/my")]
    [Authorize]
    public async Task<ActionResult<object>> GetVisitorReservations()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        return Ok(await _visitor.GetVisitorReservationsAsync(visitorId.Value));
    }

    /// <summary>
    /// Ziyaretcinin kendi rezervasyon detayini getir
    /// </summary>
    [HttpGet("visitor/my/{id}")]
    [Authorize]
    public async Task<ActionResult<object>> GetVisitorReservationDetail(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var result = await _visitor.GetVisitorReservationDetailAsync(visitorId.Value, id);
        if (result == null)
            return NotFound(new { message = "Error.ReservationNotFound" });

        return Ok(result);
    }

    /// <summary>
    /// Ziyaretci kendi rezervasyonunu iptal et
    /// </summary>
    [HttpPut("visitor/my/{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelVisitorReservation(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, result, statusCode) = await _visitor.CancelVisitorReservationAsync(visitorId.Value, id);
        return StatusCode(statusCode, result);
    }

    [HttpPut("visitor/my/{id}/change-date")]
    [Authorize]
    public async Task<IActionResult> ChangeDateVisitorReservation(int id, [FromBody] ChangeDateRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Error.LoginRequired" });

        var (success, message) = await _visitor.ChangeDateAsync(visitorId.Value, id, request.NewStartDate);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPut("{id}/photo-link")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<IActionResult> UpdatePhotoLink(int id, [FromBody] PhotoLinkRequest request)
    {
        var (success, message) = await _visitor.UpdatePhotoLinkAsync(id, request.PhotoLink);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _crud.GetByIdAsync(id);
        if (reservation == null) return NotFound();
        return reservation;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
    {
        var created = await _crud.CreateAsync(reservation);
        return CreatedAtAction(nameof(GetReservation), new { id = created.Id }, created);
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
        var visitorId = GetVisitorId();

        // Firma hesaplari (CompanyOwner, Staff, Admin) rezervasyon yapamaz
        var userTypeStr = User.FindFirst("UserTypeId")?.Value;
        if (int.TryParse(userTypeStr, out var userTypeId) && userTypeId >= 1)
            return BadRequest(new { message = "Error.CompanyCannotReserve" });

        var customerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var (success, result, statusCode) = await _payment.CreatePublicReservationAsync(
            visitorId,
            request.TourId,
            request.FullName,
            request.Email,
            request.Phone,
            request.NumberOfPeople,
            request.Notes,
            request.Address,
            request.TourDateId,
            request.StartDate,
            customerIp,
            request.CouponCode);

        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Odeme callback - iyzico'dan gelen sonucu isle
    /// </summary>
    [HttpPost("payment/callback")]
    public async Task<ActionResult<object>> PaymentCallback([FromForm] string token, [FromForm] int? reservationId = null)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Error.TokenRequired" });

        var (success, result, statusCode) = await _payment.ProcessPaymentCallbackAsync(token, reservationId);
        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Odeme durumunu sorgula
    /// </summary>
    [HttpGet("payment/status/{reservationId}")]
    public async Task<ActionResult<object>> GetPaymentStatus(int reservationId)
    {
        var result = await _payment.GetPaymentStatusAsync(reservationId);
        if (result == null)
            return NotFound(new { message = "Error.ReservationNotFound" });

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateReservation(int id, Reservation reservation)
    {
        if (id != reservation.Id) return BadRequest();
        await _crud.UpdateAsync(reservation);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int status)
    {
        var found = await _crud.UpdateStatusAsync(id, status);
        if (!found) return NotFound();
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
    public int? TourDateId { get; set; }
    public DateTime? StartDate { get; set; }
    public string? CouponCode { get; set; }
    public string? ParticipantInfo { get; set; }
}

public class ChangeDateRequest
{
    public DateTime NewStartDate { get; set; }
}

public class PhotoLinkRequest
{
    public string PhotoLink { get; set; } = string.Empty;
}
