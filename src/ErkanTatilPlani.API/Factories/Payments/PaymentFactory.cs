using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Payments;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Payments;

public class PaymentFactory : IPaymentFactory
{
    private readonly IReservationEntityService _reservationService;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentFactory> _logger;

    public PaymentFactory(
        IReservationEntityService reservationService,
        IPaymentService paymentService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<PaymentFactory> logger)
    {
        _reservationService = reservationService;
        _paymentService = paymentService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(bool success, object result, int statusCode)> InitializePaymentAsync(int visitorId, int reservationId, string scheme, string host)
    {
        var reservation = await _reservationService.GetActiveReservations()
            .Include(r => r.Tour).ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.VisitorId == visitorId);

        if (reservation == null)
            return (false, new { message = "Rezervasyon bulunamadi" }, 404);

        if (reservation.PaymentStatus == PaymentStatusEnum.FullyPaid)
            return (false, new { message = "Bu rezervasyon zaten odenmis" }, 400);

        if (reservation.Status == ReservationStatus.Cancelled)
            return (false, new { message = "Iptal edilmis rezervasyon icin odeme yapilamaz" }, 400);

        var callbackUrl = $"{scheme}://{host}/api/payments/callback";
        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            Amount = reservation.TotalPrice,
            CustomerEmail = reservation.Visitor.Email,
            CustomerName = reservation.Visitor.FirstName,
            CustomerSurname = reservation.Visitor.LastName,
            CustomerPhone = reservation.Visitor.Phone ?? "",
            CustomerIp = "127.0.0.1",
            CustomerAddress = reservation.Visitor.Address ?? "",
            ProductName = reservation.Tour.Name,
            ProductCategory = "Tur",
            CallbackUrl = callbackUrl
        };

        var result = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (result.Success)
        {
            reservation.PaymentToken = result.Token;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return (true, new { success = true, paymentPageUrl = result.PaymentPageUrl, token = result.Token }, 200);
        }

        return (false, new { success = false, message = result.ErrorMessage, errorCode = result.ErrorCode }, 400);
    }

    public async Task<(bool success, object result, int statusCode)> InitializeRemainingPaymentAsync(int visitorId, int reservationId, string scheme, string host)
    {
        var reservation = await _reservationService.GetActiveReservations()
            .Include(r => r.Tour).ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.VisitorId == visitorId);

        if (reservation == null)
            return (false, new { message = "Rezervasyon bulunamadi" }, 404);

        if (reservation.PaymentStatus != PaymentStatusEnum.DepositPaid)
            return (false, new { message = "Bu islem icin once on odeme yapilmis olmalidir" }, 400);

        var remainingAmount = reservation.TotalPrice - reservation.PaidAmount;
        if (remainingAmount <= 0)
            return (false, new { message = "Odenecek tutar kalmadi" }, 400);

        var callbackUrl = $"{scheme}://{host}/api/payments/callback";
        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            Amount = remainingAmount,
            CustomerEmail = reservation.Visitor.Email,
            CustomerName = reservation.Visitor.FirstName,
            CustomerSurname = reservation.Visitor.LastName,
            CustomerPhone = reservation.Visitor.Phone ?? "",
            CustomerIp = "127.0.0.1",
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
            await _unitOfWork.SaveChangesAsync();
            return (true, new { success = true, paymentPageUrl = result.PaymentPageUrl, token = result.Token, remainingAmount }, 200);
        }

        return (false, new { success = false, message = result.ErrorMessage, errorCode = result.ErrorCode }, 400);
    }

    public async Task<(bool success, string? redirectUrl)> ProcessCallbackAsync(string token)
    {
        _logger.LogInformation("Payment callback received for token: {Token}", token);

        var result = await _paymentService.ProcessCallbackAsync(token);

        if (result.Success)
        {
            var reservation = await _reservationService.GetByIdWithDetailsAsync(result.ReservationId);

            if (reservation != null)
            {
                reservation.PaidAmount += result.PaidAmount ?? 0;
                reservation.PaymentId = result.PaymentId;
                reservation.PaidAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                reservation.PaymentStatus = reservation.PaidAmount >= reservation.TotalPrice
                    ? PaymentStatusEnum.FullyPaid
                    : PaymentStatusEnum.DepositPaid;

                reservation.Status = ReservationStatus.Confirmed;
                await _unitOfWork.SaveChangesAsync();

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
                return (true, $"/Account/PaymentResult?status=success&reservationId={result.ReservationId}");
            }
        }
        else
        {
            if (result.ReservationId > 0)
            {
                var reservation = await _reservationService.GetByIdAsync(result.ReservationId);
                if (reservation != null)
                {
                    reservation.PaymentStatus = PaymentStatusEnum.Failed;
                    reservation.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            _logger.LogWarning("Payment failed for reservation {ReservationId}: {ErrorMessage}", result.ReservationId, result.ErrorMessage);
            return (false, $"/Account/PaymentResult?status=failed&reservationId={result.ReservationId}&error={Uri.EscapeDataString(result.ErrorMessage ?? "Odeme basarisiz")}");
        }

        return (false, "/Account/Reservations");
    }

    public async Task<object?> GetPaymentStatusAsync(int visitorId, int reservationId)
    {
        return await _reservationService.GetActiveReservations()
            .Where(r => r.Id == reservationId && r.VisitorId == visitorId)
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
    }

    public async Task<object> GetPendingPaymentsAsync(int visitorId)
    {
        var reservations = await _reservationService.GetByVisitorId(visitorId)
            .Include(r => r.Tour)
            .Where(r => r.PaymentStatus == PaymentStatusEnum.Pending && r.Status != ReservationStatus.Cancelled)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                r.TotalPrice,
                r.StartDate,
                r.CreatedAt
            })
            .ToListAsync();

        return new { reservations };
    }
}
