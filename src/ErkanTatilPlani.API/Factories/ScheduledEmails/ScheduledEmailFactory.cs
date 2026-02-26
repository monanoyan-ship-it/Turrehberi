using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.ScheduledEmails;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.ScheduledEmails;

public class ScheduledEmailFactory : IScheduledEmailFactory
{
    private readonly IScheduledEmailEntityService _emailEntityService;
    private readonly IReservationEntityService _reservationService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduledEmailFactory(
        IScheduledEmailEntityService emailEntityService,
        IReservationEntityService reservationService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _emailEntityService = emailEntityService;
        _reservationService = reservationService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task QueueReservationEmailsAsync(int reservationId)
    {
        var reservation = await _reservationService.GetByIdWithDetailsAsync(reservationId);
        if (reservation == null) return;

        var tourDate = reservation.Date.ToDateTime(TimeOnly.FromTimeSpan(reservation.StartTime));
        tourDate = DateTime.SpecifyKind(tourDate, DateTimeKind.Utc);
        var visitorEmail = reservation.Visitor.Email;
        var visitorId = reservation.VisitorId;

        var emails = new List<ScheduledEmail>
        {
            new()
            {
                VisitorId = visitorId,
                ReservationId = reservationId,
                EmailTypeId = ScheduledEmailTypes.Ids.TourDetailsReminder,
                EmailTo = visitorEmail,
                TemplateKey = "tour_reminder_3days",
                ScheduledAt = tourDate.AddDays(-3)
            },
            new()
            {
                VisitorId = visitorId,
                ReservationId = reservationId,
                EmailTypeId = ScheduledEmailTypes.Ids.DayBeforeReminder,
                EmailTo = visitorEmail,
                TemplateKey = "tour_reminder_1day",
                ScheduledAt = tourDate.AddDays(-1)
            },
            new()
            {
                VisitorId = visitorId,
                ReservationId = reservationId,
                EmailTypeId = ScheduledEmailTypes.Ids.TourMorning,
                EmailTo = visitorEmail,
                TemplateKey = "tour_morning",
                ScheduledAt = tourDate.AddHours(-2)
            },
            new()
            {
                VisitorId = visitorId,
                ReservationId = reservationId,
                EmailTypeId = ScheduledEmailTypes.Ids.ThankYou,
                EmailTo = visitorEmail,
                TemplateKey = "tour_thankyou",
                ScheduledAt = tourDate.AddDays(1)
            },
            new()
            {
                VisitorId = visitorId,
                ReservationId = reservationId,
                EmailTypeId = ScheduledEmailTypes.Ids.SimilarTourRecommendation,
                EmailTo = visitorEmail,
                TemplateKey = "tour_recommendation",
                ScheduledAt = tourDate.AddDays(7)
            }
        };

        // Gecmis tarihleri atla
        foreach (var email in emails.Where(e => e.ScheduledAt > DateTime.UtcNow))
        {
            _emailEntityService.Add(email);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelReservationEmailsAsync(int reservationId)
    {
        var pendingEmails = await _emailEntityService.GetByReservationId(reservationId)
            .Where(e => e.StatusId == ScheduledEmailStatuses.Ids.Pending)
            .ToListAsync();

        foreach (var email in pendingEmails)
        {
            email.StatusId = ScheduledEmailStatuses.Ids.Cancelled;
            email.UpdatedAt = DateTime.UtcNow;
            _emailEntityService.Update(email);
        }

        if (pendingEmails.Count > 0)
            await _unitOfWork.SaveChangesAsync();
    }

    public async Task ProcessPendingEmailsAsync()
    {
        var pendingEmails = await _emailEntityService.GetPendingEmails(DateTime.UtcNow)
            .Include(e => e.Reservation)
                .ThenInclude(r => r!.Tour)
                    .ThenInclude(t => t.Company)
            .Include(e => e.Visitor)
            .Take(50)
            .ToListAsync();

        foreach (var email in pendingEmails)
        {
            try
            {
                // Iptal edilmis rezervasyonlari atla
                if (email.Reservation != null && email.Reservation.Status == ReservationStatuses.Ids.Cancelled)
                {
                    email.StatusId = ScheduledEmailStatuses.Ids.Cancelled;
                    email.UpdatedAt = DateTime.UtcNow;
                    _emailEntityService.Update(email);
                    continue;
                }

                var templateData = BuildTemplateData(email);

                await _emailService.SendTemplateEmailAsync(
                    email.EmailTo,
                    email.TemplateKey ?? "generic",
                    email.Visitor.PreferredLanguage ?? "tr",
                    templateData);

                email.StatusId = ScheduledEmailStatuses.Ids.Sent;
                email.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                email.RetryCount++;
                email.ErrorMessage = ex.Message;
                if (email.RetryCount >= 3)
                    email.StatusId = ScheduledEmailStatuses.Ids.Failed;
            }

            email.UpdatedAt = DateTime.UtcNow;
            _emailEntityService.Update(email);
        }

        if (pendingEmails.Count > 0)
            await _unitOfWork.SaveChangesAsync();
    }

    private static Dictionary<string, string> BuildTemplateData(ScheduledEmail email)
    {
        var data = new Dictionary<string, string>
        {
            ["visitorName"] = $"{email.Visitor.FirstName} {email.Visitor.LastName}",
            ["visitorEmail"] = email.EmailTo
        };

        if (email.Reservation != null)
        {
            data["tourName"] = email.Reservation.Tour.Name;
            data["tourDestination"] = email.Reservation.Tour.Destination;
            data["companyName"] = email.Reservation.Tour.Company.Name;
            data["reservationDate"] = email.Reservation.Date.ToString("yyyy-MM-dd");
            data["startTime"] = email.Reservation.StartTime.ToString(@"hh\:mm");
            data["numberOfPeople"] = email.Reservation.NumberOfPeople.ToString();
            data["totalPrice"] = email.Reservation.TotalPrice.ToString("F2");
        }

        return data;
    }
}
