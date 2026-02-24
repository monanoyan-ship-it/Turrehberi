using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Notifications;
using ErkanTatilPlani.Core.Factories.Promotions;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ErkanTatilPlani.API.Factories.Reservations;

public class ReservationPaymentFactory : IReservationPaymentFactory
{
    private readonly ITourEntityService _tourService;
    private readonly ITourScheduleEntityService _scheduleService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IPromotionCalculationFactory _promotionCalculation;
    private readonly IPromotionEntityService _promotionService;
    private readonly INotificationFactory _notificationFactory;

    public ReservationPaymentFactory(
        ITourEntityService tourService,
        ITourScheduleEntityService scheduleService,
        IVisitorEntityService visitorService,
        IReservationEntityService reservationService,
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        IEmailService emailService,
        IConfiguration configuration,
        IPromotionCalculationFactory promotionCalculation,
        IPromotionEntityService promotionService,
        INotificationFactory notificationFactory)
    {
        _tourService = tourService;
        _scheduleService = scheduleService;
        _visitorService = visitorService;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _emailService = emailService;
        _configuration = configuration;
        _promotionCalculation = promotionCalculation;
        _promotionService = promotionService;
        _notificationFactory = notificationFactory;
    }

    public async Task<(bool success, object result, int statusCode)> CreatePublicReservationAsync(
        int? visitorId,
        int tourId,
        string fullName,
        string email,
        string phone,
        int numberOfPeople,
        string? notes,
        string? address,
        int? tourDateId,
        DateTime? startDate,
        string customerIp,
        string? couponCode = null,
        string? dateToken = null,
        bool payFullAmount = false)
    {
        // Tur kontrolu
        var tour = await _tourService.GetByIdWithCompanyAsync(tourId);
        if (tour == null)
            return (false, new { message = "Error.TourNotFound" }, 404);

        if (tour.Company == null || tour.Company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, new { message = "Error.TourNotAvailableForReservation" }, 400);

        // Validasyon
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, new { message = "Validation.FullNameRequired" }, 400);
        if (string.IsNullOrWhiteSpace(email))
            return (false, new { message = "Validation.EmailRequired" }, 400);
        if (string.IsNullOrWhiteSpace(phone))
            return (false, new { message = "Validation.PhoneRequired" }, 400);
        if (numberOfPeople < 1)
            return (false, new { message = "Validation.MinOnePersonRequired" }, 400);

        // Giris yapmamis kullanici icin visitor olustur/bul
        if (visitorId == null)
        {
            var existingVisitor = await _visitorService.GetActiveByEmailAsync(email);
            if (existingVisitor != null)
            {
                visitorId = existingVisitor.Id;
            }
            else
            {
                var nameParts = fullName.Trim().Split(' ', 2);
                var newVisitor = new Visitor
                {
                    FirstName = nameParts[0],
                    LastName = nameParts.Length > 1 ? nameParts[1] : "",
                    Email = email,
                    Phone = phone,
                    PasswordHash = "",
                    UserTypeId = UserTypes.Ids.Visitor
                };
                _visitorService.Add(newVisitor);
                await _unitOfWork.SaveChangesAsync();
                visitorId = newVisitor.Id;
            }
        }

        // dateToken'dan schedule ve tarih bilgisi coz
        DateOnly resolvedDate;
        TimeSpan resolvedStartTime;
        int resolvedDurationValue;
        int resolvedDurationUnitId;
        decimal resolvedUnitPrice;
        int? resolvedScheduleId = null;

        if (!string.IsNullOrEmpty(dateToken) && dateToken.StartsWith("s:"))
        {
            // "s:42:2026-03-15" formatindan parse et
            var parts = dateToken.Split(':');
            if (parts.Length != 3 || !int.TryParse(parts[1], out var scheduleId) || !DateOnly.TryParse(parts[2], out var parsedDate))
                return (false, new { message = "Error.InvalidDateToken" }, 400);

            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null || !schedule.IsActive)
                return (false, new { message = "Error.ScheduleNotFound" }, 404);

            if (schedule.TourId != tourId)
                return (false, new { message = "Error.SessionNotBelongToTour" }, 400);

            // Gecerlilik kontrolu
            var dateAsDateTime = parsedDate.ToDateTime(TimeOnly.MinValue);
            dateAsDateTime = DateTime.SpecifyKind(dateAsDateTime, DateTimeKind.Utc);
            if (dateAsDateTime < schedule.ValidFrom.Date || dateAsDateTime > schedule.ValidTo.Date)
                return (false, new { message = "Error.DateOutOfRange" }, 400);

            // Gun eslesmesi kontrolu
            var daysOfWeek = ParseDaysOfWeek(schedule.DaysOfWeekJson);
            if (!daysOfWeek.Contains((int)dateAsDateTime.DayOfWeek))
                return (false, new { message = "Error.DateNotMatchSchedule" }, 400);

            // Kapasite kontrolu
            var maxCapacity = schedule.MaxCapacity ?? tour.MaxCapacity;
            var currentBooked = await _reservationService.GetActiveReservations()
                .Where(r => r.ScheduleId == scheduleId &&
                            r.Date == parsedDate &&
                            r.Status != ReservationStatuses.Ids.Cancelled)
                .SumAsync(r => r.NumberOfPeople);

            if (maxCapacity > 0 && currentBooked + numberOfPeople > maxCapacity)
                return (false, new { message = "Error.InsufficientCapacity" }, 400);

            resolvedDate = parsedDate;
            resolvedStartTime = schedule.StartTime;
            resolvedDurationValue = schedule.DurationValue;
            resolvedDurationUnitId = schedule.DurationUnitId;
            resolvedUnitPrice = schedule.Price;
            resolvedScheduleId = scheduleId;
        }
        else
        {
            // Legacy fallback
            var fallbackDate = startDate ?? DateTime.UtcNow.AddDays(7);
            resolvedDate = DateOnly.FromDateTime(fallbackDate);
            resolvedStartTime = new TimeSpan(9, 0, 0);
            resolvedDurationValue = tour.DurationDays;
            resolvedDurationUnitId = DurationUnits.Ids.Day;
            resolvedUnitPrice = tour.Price;
        }

        // Promosyon destekli fiyat hesaplama
        var resolvedStartDateTime = resolvedDate.ToDateTime(TimeOnly.FromTimeSpan(resolvedStartTime));
        resolvedStartDateTime = DateTime.SpecifyKind(resolvedStartDateTime, DateTimeKind.Utc);

        var priceResult = await _promotionCalculation.CalculatePriceAsync(
            tourId, numberOfPeople, resolvedStartDateTime, couponCode, visitorId);

        var totalPrice = priceResult.FinalPrice;
        var depositAmount = priceResult.DepositAmount;
        var discountAmount = priceResult.TotalDiscount;

        // Rezervasyon olustur
        var reservation = new Reservation
        {
            TourId = tourId,
            VisitorId = visitorId!.Value,
            Date = resolvedDate,
            StartTime = resolvedStartTime,
            DurationValue = resolvedDurationValue,
            DurationUnitId = resolvedDurationUnitId,
            UnitPrice = resolvedUnitPrice,
            ScheduleId = resolvedScheduleId,
            NumberOfPeople = numberOfPeople,
            TotalPrice = totalPrice,
            DepositAmount = depositAmount,
            DiscountAmount = discountAmount,
            CouponCode = couponCode,
            AppliedPromotions = priceResult.AppliedDiscounts.Count > 0
                ? JsonSerializer.Serialize(priceResult.AppliedDiscounts)
                : null,
            PromotionId = priceResult.AppliedDiscounts.Count > 0
                ? priceResult.AppliedDiscounts.First().PromotionId
                : null,
            PaidAmount = 0,
            Status = ReservationStatuses.Ids.Pending,
            PaymentStatus = PaymentStatuses.Ids.Pending,
            Notes = notes ?? ""
        };

        _reservationService.Add(reservation);
        await _unitOfWork.SaveChangesAsync();

        // Promosyon kullanim kaydlari
        foreach (var applied in priceResult.AppliedDiscounts)
        {
            _promotionService.AddUsage(new PromotionUsage
            {
                PromotionId = applied.PromotionId,
                ReservationId = reservation.Id,
                VisitorId = visitorId,
                DiscountAmount = applied.DiscountAmount,
                AppliedRule = applied.Rule
            });
        }

        // Kupon kullanim sayacini artir
        if (!string.IsNullOrEmpty(couponCode))
        {
            var couponPromo = priceResult.AppliedDiscounts
                .FirstOrDefault(d => d.PromotionType == "Coupon");
            if (couponPromo != null)
            {
                var coupon = await _promotionService.GetByIdAsync(couponPromo.PromotionId);
                if (coupon != null)
                {
                    coupon.UsageCount++;
                    _promotionService.Update(coupon);
                }
            }
        }

        // Flash sale sold count artir
        foreach (var fs in priceResult.AppliedDiscounts.Where(d => d.PromotionType == "FlashSale"))
        {
            var flashPromo = await _promotionService.GetByIdAsync(fs.PromotionId);
            if (flashPromo != null)
            {
                flashPromo.FlashSaleSoldCount++;
                _promotionService.Update(flashPromo);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Odeme baslatma
        var webBaseUrl = _configuration["WebBaseUrl"] ?? "https://localhost:7080";
        var callbackUrl = $"{webBaseUrl}/Account/PaymentResult?reservationId={reservation.Id}";

        var nameParts2 = fullName.Trim().Split(' ', 2);
        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            Amount = payFullAmount ? totalPrice : depositAmount,
            CustomerEmail = email,
            CustomerName = nameParts2[0],
            CustomerSurname = nameParts2.Length > 1 ? nameParts2[1] : "",
            CustomerPhone = phone,
            CustomerIp = customerIp,
            CustomerAddress = address ?? "",
            ProductName = $"{tour.Name} - {tour.Destination} ({numberOfPeople} pax)",
            ProductCategory = "Tour Reservation",
            CallbackUrl = callbackUrl
        };

        var paymentResult = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (!paymentResult.Success)
        {
            _reservationService.Remove(reservation);
            await _unitOfWork.SaveChangesAsync();

            return (false, new
            {
                message = "Error.PaymentInitFailed",
                error = paymentResult.ErrorMessage
            }, 400);
        }

        // Payment token'i kaydet
        reservation.PaymentToken = paymentResult.Token;
        await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            success = true,
            reservationId = reservation.Id,
            paymentPageUrl = paymentResult.PaymentPageUrl,
            totalPrice,
            depositAmount,
            depositPercentage = priceResult.DepositPercentage,
            discountAmount,
            originalPrice = priceResult.SubTotal,
            appliedDiscounts = priceResult.AppliedDiscounts
        }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> ProcessPaymentCallbackAsync(string token, int? reservationId)
    {
        var paymentResult = await _paymentService.ProcessCallbackAsync(token);

        Reservation? reservation = null;

        if (paymentResult.ReservationId > 0)
            reservation = await _reservationService.GetByIdWithDetailsAsync(paymentResult.ReservationId);

        if (reservation == null)
            reservation = await _reservationService.GetByPaymentTokenAsync(token);

        if (reservation == null && reservationId.HasValue && reservationId.Value > 0)
            reservation = await _reservationService.GetByIdWithDetailsAsync(reservationId.Value);

        if (reservation == null)
            return (false, new { message = "Error.ReservationNotFound", token = token.Substring(0, Math.Min(20, token.Length)) }, 404);

        if (paymentResult.Success)
        {
            reservation.PaidAmount += paymentResult.PaidAmount ?? 0;
            reservation.PaymentId = paymentResult.PaymentId;
            reservation.PaidAt = DateTime.UtcNow;
            reservation.Status = ReservationStatuses.Ids.Confirmed;
            reservation.QrToken = Guid.NewGuid().ToString("N");

            if (reservation.PaidAmount >= reservation.TotalPrice)
                reservation.PaymentStatus = PaymentStatuses.Ids.FullyPaid;
            else
                reservation.PaymentStatus = PaymentStatuses.Ids.DepositPaid;

            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _notificationFactory.CreateReservationNotificationAsync(
                    reservation.VisitorId, reservation.Id, "confirmed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bildirim hatasi: {ex.Message}");
            }

            try
            {
                var activeCount = await _reservationService.GetActiveReservations()
                    .CountAsync(r => r.TourId == reservation.TourId
                        && (r.Status == ReservationStatuses.Ids.Pending || r.Status == ReservationStatuses.Ids.Confirmed));
                var tour = reservation.Tour;
                var remaining = tour.MaxCapacity - activeCount;
                if (remaining > 0 && remaining <= 5)
                    await _notificationFactory.CreateScarcityNotificationsAsync(tour.Id, remaining);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kitlik bildirimi hatasi: {ex.Message}");
            }

            try
            {
                var emailModel = new ReservationEmailModel
                {
                    ToEmail = reservation.Visitor.Email,
                    CustomerName = $"{reservation.Visitor.FirstName} {reservation.Visitor.LastName}",
                    TourName = reservation.Tour.Name,
                    CompanyName = reservation.Tour.Company.Name,
                    Destination = reservation.Tour.Destination,
                    Date = reservation.Date,
                    StartTime = reservation.StartTime,
                    DurationValue = reservation.DurationValue,
                    DurationUnitId = reservation.DurationUnitId,
                    NumberOfPeople = reservation.NumberOfPeople,
                    TotalPrice = reservation.TotalPrice,
                    Notes = reservation.Notes,
                    PreferredLanguage = reservation.Visitor.PreferredLanguage ?? "tr"
                };
                await _emailService.SendReservationConfirmedEmailAsync(emailModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email gonderme hatasi: {ex.Message}");
            }

            return (true, new
            {
                success = true,
                message = "Success.PaymentCompleted",
                reservationId = reservation.Id,
                paymentId = paymentResult.PaymentId,
                qrToken = reservation.QrToken
            }, 200);
        }
        else
        {
            reservation.PaymentStatus = PaymentStatuses.Ids.Failed;
            await _unitOfWork.SaveChangesAsync();

            return (true, new
            {
                success = false,
                message = paymentResult.ErrorMessage ?? "Error.PaymentFailed",
                reservationId = reservation.Id
            }, 200);
        }
    }

    public async Task<object?> GetPaymentStatusAsync(int reservationId)
    {
        var reservation = await _reservationService.GetActiveReservations()
            .Include(r => r.Tour)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            return null;

        return new
        {
            reservationId = reservation.Id,
            paymentStatus = (PaymentStatuses.GetById(reservation.PaymentStatus)?.SystemName ?? "Unknown"),
            reservationStatus = (ReservationStatuses.GetById(reservation.Status)?.SystemName ?? "Unknown"),
            totalPrice = reservation.TotalPrice,
            paidAt = reservation.PaidAt,
            tourName = reservation.Tour.Name
        };
    }

    private static List<int> ParseDaysOfWeek(string json)
    {
        try { return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>(); }
        catch { return new List<int>(); }
    }
}
