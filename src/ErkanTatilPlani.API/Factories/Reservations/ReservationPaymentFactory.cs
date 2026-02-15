using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ErkanTatilPlani.API.Factories.Reservations;

public class ReservationPaymentFactory : IReservationPaymentFactory
{
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ReservationPaymentFactory(
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        IReservationEntityService reservationService,
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _tourService = tourService;
        _visitorService = visitorService;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _emailService = emailService;
        _configuration = configuration;
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
        DateTime? startDate,
        string customerIp)
    {
        // Tur kontrolu
        var tour = await _tourService.GetByIdWithCompanyAsync(tourId);
        if (tour == null)
            return (false, new { message = "Tur bulunamadi" }, 404);

        if (tour.Company == null || tour.Company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, new { message = "Bu tur su anda rezervasyona kapali" }, 400);

        // Validasyon
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, new { message = "Ad soyad zorunludur" }, 400);
        if (string.IsNullOrWhiteSpace(email))
            return (false, new { message = "E-posta zorunludur" }, 400);
        if (string.IsNullOrWhiteSpace(phone))
            return (false, new { message = "Telefon zorunludur" }, 400);
        if (numberOfPeople < 1)
            return (false, new { message = "Kisi sayisi en az 1 olmalidir" }, 400);

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

        // Toplam fiyat hesapla
        var totalPrice = tour.Price * numberOfPeople;

        // On odeme tutarini hesapla
        var depositPercentage = tour.Company.DepositPercentage;
        var depositAmount = totalPrice * depositPercentage / 100;

        // Baslangic ve bitis tarihi
        var resolvedStartDate = startDate ?? DateTime.UtcNow.AddDays(7);
        var endDate = resolvedStartDate.AddDays(tour.DurationDays);

        // Rezervasyon olustur
        var reservation = new Reservation
        {
            TourId = tourId,
            VisitorId = visitorId!.Value,
            StartDate = resolvedStartDate,
            EndDate = endDate,
            NumberOfPeople = numberOfPeople,
            TotalPrice = totalPrice,
            DepositAmount = depositAmount,
            PaidAmount = 0,
            Status = ReservationStatuses.Ids.Pending,
            PaymentStatus = PaymentStatuses.Ids.Pending,
            Notes = notes ?? ""
        };

        _reservationService.Add(reservation);
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
            CustomerAddress = address ?? "Belirtilmedi",
            ProductName = $"{tour.Name} - {tour.Destination} ({numberOfPeople} kisi)",
            ProductCategory = "Tur Rezervasyonu",
            CallbackUrl = callbackUrl
        };

        var paymentResult = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (!paymentResult.Success)
        {
            _reservationService.Remove(reservation);
            await _unitOfWork.SaveChangesAsync();

            return (false, new
            {
                message = "Odeme baslatilirken bir hata olustu",
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
            depositPercentage
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
            return (false, new { message = "Rezervasyon bulunamadi", token = token.Substring(0, Math.Min(20, token.Length)) }, 404);

        if (paymentResult.Success)
        {
            // Odeme basarili - odenen tutari guncelle
            reservation.PaidAmount += paymentResult.PaidAmount ?? 0;
            reservation.PaymentId = paymentResult.PaymentId;
            reservation.PaidAt = DateTime.UtcNow;
            reservation.Status = ReservationStatuses.Ids.Confirmed;

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
                message = "Odeme basarili",
                reservationId = reservation.Id,
                paymentId = paymentResult.PaymentId
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
                message = paymentResult.ErrorMessage ?? "Odeme basarisiz",
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
