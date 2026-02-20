using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;
using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.API.Factories.Reservations;

public class VisitorReservationFactory : IVisitorReservationFactory
{
    private readonly IReservationEntityService _reservationService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ITourEntityService _tourService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public VisitorReservationFactory(
        IReservationEntityService reservationService,
        IVisitorEntityService visitorService,
        ITourEntityService tourService,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _reservationService = reservationService;
        _visitorService = visitorService;
        _tourService = tourService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<object> GetVisitorReservationsAsync(int visitorId)
    {
        var reservations = await _reservationService.GetByVisitorId(visitorId)
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
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
                Status = r.Status == ReservationStatuses.Ids.Pending ? "Pending" : r.Status == ReservationStatuses.Ids.Confirmed ? "Confirmed" : r.Status == ReservationStatuses.Ids.Cancelled ? "Cancelled" : r.Status == ReservationStatuses.Ids.Completed ? "Completed" : "Unknown",
                StatusId = r.Status,
                r.Notes,
                r.CreatedAt
            })
            .ToListAsync();

        return new { reservations };
    }

    public async Task<object?> GetVisitorReservationDetailAsync(int visitorId, int reservationId)
    {
        var reservation = await _reservationService.GetByVisitorId(visitorId)
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Where(r => r.Id == reservationId)
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
                Status = r.Status == ReservationStatuses.Ids.Pending ? "Pending" : r.Status == ReservationStatuses.Ids.Confirmed ? "Confirmed" : r.Status == ReservationStatuses.Ids.Cancelled ? "Cancelled" : r.Status == ReservationStatuses.Ids.Completed ? "Completed" : "Unknown",
                StatusId = r.Status,
                r.Notes,
                r.CreatedAt,
                r.UpdatedAt,
                CanCancel = r.Status == ReservationStatuses.Ids.Pending || r.Status == ReservationStatuses.Ids.Confirmed,
                // Odeme bilgileri
                r.DepositAmount,
                r.PaidAmount,
                r.PaymentId,
                PaymentStatus = r.PaymentStatus == PaymentStatuses.Ids.Pending ? "Pending" : r.PaymentStatus == PaymentStatuses.Ids.DepositPaid ? "DepositPaid" : r.PaymentStatus == PaymentStatuses.Ids.FullyPaid ? "FullyPaid" : r.PaymentStatus == PaymentStatuses.Ids.Failed ? "Failed" : r.PaymentStatus == PaymentStatuses.Ids.Refunded ? "Refunded" : "Unknown",
                PaymentStatusId = r.PaymentStatus,
                r.PaidAt
            })
            .FirstOrDefaultAsync();

        return reservation;
    }

    public async Task<(bool success, object result, int statusCode)> CancelVisitorReservationAsync(int visitorId, int reservationId)
    {
        var reservation = await _reservationService.GetByVisitorId(visitorId)
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            return (false, new { message = "Rezervasyon bulunamadi" }, 404);

        // Sadece Pending veya Confirmed durumundaki rezervasyonlar iptal edilebilir
        if (reservation.Status != ReservationStatuses.Ids.Pending && reservation.Status != ReservationStatuses.Ids.Confirmed)
            return (false, new { message = "Bu rezervasyon iptal edilemez" }, 400);

        reservation.Status = ReservationStatuses.Ids.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

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

        return (true, new { message = "Rezervasyon iptal edildi" }, 200);
    }

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetMyReservationsAsync(int visitorId, string? status)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null)
            return (null, "Kullanici bulunamadi", null, 401);
        if (visitor.Company == null)
            return (null, "Firma sahibi degilsiniz", "NOT_COMPANY_OWNER", 403);

        // Firmanin turlarinin ID'leri
        var companyTours = await _tourService.GetByCompanyIdAsync(visitor.Company.Id);
        var companyTourIds = companyTours.Select(t => t.Id).ToList();

        var query = _reservationService.GetByTourIds(companyTourIds)
            .Include(r => r.Tour)
            .Include(r => r.Visitor);

        // Durum filtresi
        IQueryable<Reservation> filteredQuery = query;
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            var statusItem = ReservationStatuses.GetBySystemName(status); if (statusItem != null)
            {
                filteredQuery = filteredQuery.Where(r => r.Status == statusItem!.Id);
            }
        }

        var reservations = await filteredQuery
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
                Status = r.Status == ReservationStatuses.Ids.Pending ? "Pending" : r.Status == ReservationStatuses.Ids.Confirmed ? "Confirmed" : r.Status == ReservationStatuses.Ids.Cancelled ? "Cancelled" : r.Status == ReservationStatuses.Ids.Completed ? "Completed" : "Unknown",
                StatusId = r.Status,
                r.Notes,
                r.CreatedAt
            })
            .ToListAsync();

        // Istatistikler
        var allReservations = await _reservationService.GetByTourIds(companyTourIds).ToListAsync();

        var stats = new
        {
            total = allReservations.Count,
            pending = allReservations.Count(r => r.Status == ReservationStatuses.Ids.Pending),
            confirmed = allReservations.Count(r => r.Status == ReservationStatuses.Ids.Confirmed),
            cancelled = allReservations.Count(r => r.Status == ReservationStatuses.Ids.Cancelled),
            completed = allReservations.Count(r => r.Status == ReservationStatuses.Ids.Completed)
        };

        return (new { reservations, stats }, null, null, null);
    }

    public async Task<(bool success, object? result, int statusCode)> UpdateMyReservationStatusAsync(int visitorId, int reservationId, string status, string? rejectionReason)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null)
            return (false, new { message = "Kullanici bulunamadi" }, 401);
        if (visitor.Company == null)
            return (false, new { message = "Firma sahibi degilsiniz" }, 403);

        var reservation = await _reservationService.GetByIdWithDetailsAsync(reservationId);
        if (reservation == null)
            return (false, new { message = "Rezervasyon bulunamadi" }, 404);

        // Rezervasyonun firmaya ait oldugunu kontrol et
        if (reservation.Tour.CompanyId != visitor.Company.Id)
            return (false, new { message = "Bu rezervasyonu duzenleme yetkiniz yok" }, 403);

        var newStatusItem = ReservationStatuses.GetBySystemName(status); if (newStatusItem == null)
            return (false, new { message = "Gecersiz durum" }, 400);

        var oldStatus = reservation.Status;
        reservation.Status = newStatusItem!.Id;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Email bildirimi gonder
        if (oldStatus != newStatusItem!.Id)
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
                RejectionReason = rejectionReason,
                PreferredLanguage = reservation.Visitor.PreferredLanguage ?? "tr"
            };

            if (newStatusItem!.Id == ReservationStatuses.Ids.Confirmed)
            {
                await _emailService.SendReservationConfirmedEmailAsync(emailModel);
            }
            else if (newStatusItem!.Id == ReservationStatuses.Ids.Cancelled)
            {
                await _emailService.SendReservationCancelledEmailAsync(emailModel);
            }
        }

        return (true, new { message = "Rezervasyon durumu guncellendi", status = newStatusItem!.SystemName }, 200);
    }

    public async Task<(bool success, string message)> ChangeDateAsync(int visitorId, int reservationId, DateTime newStartDate)
    {
        var reservation = await _reservationService.GetByIdAsync(reservationId);
        if (reservation == null || reservation.VisitorId != visitorId)
            return (false, "Rezervasyon bulunamadi");

        if (reservation.Status != ReservationStatuses.Ids.Confirmed)
            return (false, "Sadece onaylanmis rezervasyonlar icin tarih degistirilebilir");

        var tour = await _tourService.GetByIdAsync(reservation.TourId);
        if (tour == null)
            return (false, "Tur bulunamadi");

        reservation.StartDate = DateTime.SpecifyKind(newStartDate, DateTimeKind.Utc);
        reservation.EndDate = DateTime.SpecifyKind(newStartDate.AddDays(tour.DurationDays - 1), DateTimeKind.Utc);
        reservation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, "Tarih basariyla degistirildi");
    }

    public async Task<(bool success, string message)> UpdatePhotoLinkAsync(int reservationId, string photoLink)
    {
        var reservation = await _reservationService.GetByIdAsync(reservationId);
        if (reservation == null)
            return (false, "Rezervasyon bulunamadi");

        reservation.PhotoLink = photoLink;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, "Fotograf linki guncellendi");
    }
}
