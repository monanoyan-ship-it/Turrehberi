using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Notifications;
using ErkanTatilPlani.Core.Factories.Promotions;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Factories.TourDates;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ErkanTatilPlani.API.Factories.Reservations;

public class ReservationPaymentFactory : IReservationPaymentFactory
{
    private readonly ITourEntityService _tourService;
    private readonly ITourDateEntityService _tourDateService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IPromotionCalculationFactory _promotionCalculation;
    private readonly IPromotionEntityService _promotionService;
    private readonly INotificationFactory _notificationFactory;
    private readonly ITourScheduleFactory _scheduleFactory;

    public ReservationPaymentFactory(
        ITourEntityService tourService,
        ITourDateEntityService tourDateService,
        IVisitorEntityService visitorService,
        IReservationEntityService reservationService,
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        IEmailService emailService,
        IConfiguration configuration,
        IPromotionCalculationFactory promotionCalculation,
        IPromotionEntityService promotionService,
        INotificationFactory notificationFactory,
        ITourScheduleFactory scheduleFactory)
    {
        _tourService = tourService;
        _tourDateService = tourDateService;
        _visitorService = visitorService;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _emailService = emailService;
        _configuration = configuration;
        _promotionCalculation = promotionCalculation;
        _promotionService = promotionService;
        _notificationFactory = notificationFactory;
        _scheduleFactory = scheduleFactory;
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
        string? dateToken = null)
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

        // TourDate bazli tarih cozumleme
        DateTime resolvedStartDate;
        DateTime endDate;
        TourDate? selectedTourDate = null;

        // dateToken destegi - lazy materialization
        if (!string.IsNullOrEmpty(dateToken) && !tourDateId.HasValue)
        {
            selectedTourDate = await _scheduleFactory.MaterializeDateAsync(dateToken);
            if (selectedTourDate != null)
            {
                tourDateId = selectedTourDate.Id;
            }
        }

        if (tourDateId.HasValue)
        {
            selectedTourDate ??= await _tourDateService.GetByIdAsync(tourDateId.Value);
            if (selectedTourDate == null)
                return (false, new { message = "Error.SelectedSessionNotFound" }, 404);
            if (selectedTourDate.TourId != tourId)
                return (false, new { message = "Error.SessionNotBelongToTour" }, 400);
            if (!selectedTourDate.IsAvailable)
                return (false, new { message = "Error.SessionFull" }, 400);
            if (selectedTourDate.MaxCapacity.HasValue &&
                selectedTourDate.BookedCount + numberOfPeople > selectedTourDate.MaxCapacity.Value)
                return (false, new { message = "Error.InsufficientCapacity" }, 400);

            resolvedStartDate = selectedTourDate.StartDate;
            endDate = selectedTourDate.EndDate;
        }
        else
        {
            // Legacy fallback
            resolvedStartDate = startDate ?? DateTime.UtcNow.AddDays(7);
            endDate = resolvedStartDate.AddDays(tour.DurationDays);
        }

        // Promosyon destekli fiyat hesaplama
        var priceResult = await _promotionCalculation.CalculatePriceAsync(
            tourId, numberOfPeople, resolvedStartDate, couponCode, visitorId);

        var totalPrice = priceResult.FinalPrice;
        var depositAmount = priceResult.DepositAmount;
        var discountAmount = priceResult.TotalDiscount;

        // Rezervasyon olustur
        var reservation = new Reservation
        {
            TourId = tourId,
            VisitorId = visitorId!.Value,
            TourDateId = tourDateId,
            StartDate = resolvedStartDate,
            EndDate = endDate,
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

        // TourDate BookedCount guncelle
        if (selectedTourDate != null)
        {
            selectedTourDate.BookedCount += numberOfPeople;
            if (selectedTourDate.MaxCapacity.HasValue &&
                selectedTourDate.BookedCount >= selectedTourDate.MaxCapacity.Value)
            {
                selectedTourDate.IsAvailable = false;
            }
            selectedTourDate.UpdatedAt = DateTime.UtcNow;
        }

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
            Amount = depositAmount,
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

        // 1. Oncelikle ConversationId'den gelen reservationId'yi dene
        if (paymentResult.ReservationId > 0)
        {
            reservation = await _reservationService.GetByIdWithDetailsAsync(paymentResult.ReservationId);
        }

        // 2. ConversationId'den bulunamadiysa PaymentToken ile ara
        if (reservation == null)
        {
            reservation = await _reservationService.GetByPaymentTokenAsync(token);
        }

        // 3. Son cari - form'dan gelen reservationId ile ara
        if (reservation == null && reservationId.HasValue && reservationId.Value > 0)
        {
            reservation = await _reservationService.GetByIdWithDetailsAsync(reservationId.Value);
        }

        if (reservation == null)
            return (false, new { message = "Error.ReservationNotFound", token = token.Substring(0, Math.Min(20, token.Length)) }, 404);

        if (paymentResult.Success)
        {
            // Odeme basarili - odenen tutari guncelle
            reservation.PaidAmount += paymentResult.PaidAmount ?? 0;
            reservation.PaymentId = paymentResult.PaymentId;
            reservation.PaidAt = DateTime.UtcNow;
            reservation.Status = ReservationStatuses.Ids.Confirmed;

            // QR token olustur
            reservation.QrToken = Guid.NewGuid().ToString("N");

            // Tam odeme mi on odeme mi kontrol et
            if (reservation.PaidAmount >= reservation.TotalPrice)
            {
                reservation.PaymentStatus = PaymentStatuses.Ids.FullyPaid;
            }
            else
            {
                reservation.PaymentStatus = PaymentStatuses.Ids.DepositPaid;
            }

            await _unitOfWork.SaveChangesAsync();

            // Rezervasyon onay bildirimi
            try
            {
                await _notificationFactory.CreateReservationNotificationAsync(
                    reservation.VisitorId, reservation.Id, "confirmed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bildirim hatasi: {ex.Message}");
            }

            // Kitlik bildirimi - kalan yer kontrolu
            try
            {
                var activeCount = await _reservationService.GetActiveReservations()
                    .CountAsync(r => r.TourId == reservation.TourId
                        && (r.Status == ReservationStatuses.Ids.Pending || r.Status == ReservationStatuses.Ids.Confirmed));
                var tour = reservation.Tour;
                var remaining = tour.MaxCapacity - activeCount;
                if (remaining > 0 && remaining <= 5)
                {
                    await _notificationFactory.CreateScarcityNotificationsAsync(tour.Id, remaining);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kitlik bildirimi hatasi: {ex.Message}");
            }

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
            // Odeme basarisiz
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
}
