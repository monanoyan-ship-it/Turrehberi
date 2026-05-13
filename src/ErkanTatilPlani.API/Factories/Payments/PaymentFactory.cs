using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Payments;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ErkanTatilPlani.API.Factories.Payments;

public class PaymentFactory : IPaymentFactory
{
    private readonly IReservationEntityService _reservationService;
    private readonly IMarketplaceFinanceEntityService _marketplaceFinanceService;
    private readonly IPaymentMethodEntityService _paymentMethodService;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentFactory> _logger;
    private readonly string _webBaseUrl;

    public PaymentFactory(
        IReservationEntityService reservationService,
        IMarketplaceFinanceEntityService marketplaceFinanceService,
        IPaymentMethodEntityService paymentMethodService,
        IPaymentService paymentService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<PaymentFactory> logger,
        IConfiguration configuration)
    {
        _reservationService = reservationService;
        _marketplaceFinanceService = marketplaceFinanceService;
        _paymentMethodService = paymentMethodService;
        _paymentService = paymentService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _webBaseUrl = configuration["WebBaseUrl"] ?? "https://localhost:7080";
    }

    public async Task<(bool success, object result, int statusCode)> InitializePaymentAsync(int visitorId, int reservationId, string scheme, string host, string? paymentMethodSystemName = null)
    {
        var reservation = await _reservationService.GetActiveReservations()
            .Include(r => r.Tour).ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.VisitorId == visitorId);

        if (reservation == null)
            return (false, new { message = "Error.ReservationNotFound" }, 404);

        if (reservation.PaymentStatus == PaymentStatuses.Ids.FullyPaid)
            return (false, new { message = "Error.ReservationAlreadyPaid" }, 400);

        if (reservation.Status == ReservationStatuses.Ids.Cancelled)
            return (false, new { message = "Error.CannotPayCancelledReservation" }, 400);

        var (methodFound, method, errorResult, errorStatusCode) = await ResolveCheckoutMethodAsync(paymentMethodSystemName);
        if (!methodFound || method == null)
            return (false, errorResult, errorStatusCode);

        var callbackUrl = $"{scheme}://{host}/api/payments/callback";
        var (transaction, paymentRequest) = await CreateMarketplacePaymentRequestAsync(
            reservation,
            reservation.TotalPrice,
            PaymentTransactionTypes.Ids.FullPayment,
            reservation.Tour.Name,
            callbackUrl,
            method);

        var result = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (result.Success)
        {
            reservation.PaymentToken = result.Token;
            reservation.PaymentMethodSystemName = method.SystemName;
            reservation.PaymentProviderSystemName = method.ProviderSystemName;
            reservation.UpdatedAt = DateTime.UtcNow;
            transaction.PaymentToken = result.Token;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return (true, new { success = true, paymentPageUrl = result.PaymentPageUrl, token = result.Token }, 200);
        }

        transaction.StatusId = MarketplaceTransactionStatuses.Ids.Failed;
        transaction.ErrorCode = result.ErrorCode;
        transaction.ErrorMessage = result.ErrorMessage;
        transaction.FailedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (false, new { success = false, message = result.ErrorMessage, errorCode = result.ErrorCode }, 400);
    }

    public async Task<(bool success, object result, int statusCode)> InitializeRemainingPaymentAsync(int visitorId, int reservationId, string scheme, string host, string? paymentMethodSystemName = null)
    {
        var reservation = await _reservationService.GetActiveReservations()
            .Include(r => r.Tour).ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.VisitorId == visitorId);

        if (reservation == null)
            return (false, new { message = "Error.ReservationNotFound" }, 404);

        if (reservation.PaymentStatus != PaymentStatuses.Ids.DepositPaid)
            return (false, new { message = "Error.DepositPaymentRequired" }, 400);

        var remainingAmount = reservation.TotalPrice - reservation.PaidAmount;
        if (remainingAmount <= 0)
            return (false, new { message = "Error.NoRemainingBalance" }, 400);

        var requestedMethod = string.IsNullOrWhiteSpace(paymentMethodSystemName)
            ? reservation.PaymentMethodSystemName
            : paymentMethodSystemName;
        var (methodFound, method, errorResult, errorStatusCode) = await ResolveCheckoutMethodAsync(requestedMethod);
        if (!methodFound || method == null)
            return (false, errorResult, errorStatusCode);

        var callbackUrl = $"{scheme}://{host}/api/payments/callback";
        var (transaction, paymentRequest) = await CreateMarketplacePaymentRequestAsync(
            reservation,
            remainingAmount,
            PaymentTransactionTypes.Ids.RemainingBalance,
            $"{reservation.Tour.Name} - Remaining Payment",
            callbackUrl,
            method);

        var result = await _paymentService.InitializePaymentAsync(paymentRequest);

        if (result.Success)
        {
            reservation.PaymentToken = result.Token;
            reservation.PaymentMethodSystemName = method.SystemName;
            reservation.PaymentProviderSystemName = method.ProviderSystemName;
            reservation.UpdatedAt = DateTime.UtcNow;
            transaction.PaymentToken = result.Token;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return (true, new { success = true, paymentPageUrl = result.PaymentPageUrl, token = result.Token, remainingAmount }, 200);
        }

        transaction.StatusId = MarketplaceTransactionStatuses.Ids.Failed;
        transaction.ErrorCode = result.ErrorCode;
        transaction.ErrorMessage = result.ErrorMessage;
        transaction.FailedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (false, new { success = false, message = result.ErrorMessage, errorCode = result.ErrorCode }, 400);
    }

    public async Task<(bool success, string? redirectUrl)> ProcessCallbackAsync(string token)
    {
        _logger.LogInformation("Payment callback received for token: {Token}", token);

        var transaction = await _marketplaceFinanceService.GetTransactionByTokenAsync(token);
        var credentials = await ResolveProviderCredentialsAsync(transaction?.PaymentMethodSystemName, transaction?.Provider);
        var result = await _paymentService.ProcessCallbackAsync(token, credentials);
        var reservationId = result.ReservationId > 0 ? result.ReservationId : transaction?.ReservationId ?? 0;

        if (result.Success)
        {
            var reservation = transaction?.Reservation ?? await _reservationService.GetByIdWithDetailsAsync(reservationId);

            if (reservation != null)
            {
                reservation.PaidAmount += result.PaidAmount ?? 0;
                reservation.PaymentId = result.PaymentId;
                reservation.PaidAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                reservation.PaymentStatus = reservation.PaidAmount >= reservation.TotalPrice
                    ? PaymentStatuses.Ids.FullyPaid
                    : PaymentStatuses.Ids.DepositPaid;

                reservation.Status = ReservationStatuses.Ids.Confirmed;
                UpdateSuccessfulMarketplaceTransaction(transaction, result, reservation);
                await _unitOfWork.SaveChangesAsync();

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
                    PreferredLanguage = reservation.Visitor.PreferredLanguage ?? "tr"
                };
                await _emailService.SendReservationConfirmedEmailAsync(emailModel);

                _logger.LogInformation("Payment successful for reservation {ReservationId}", reservation.Id);
                return (true, $"{_webBaseUrl}/Account/PaymentResult?status=success&reservationId={reservation.Id}");
            }
        }
        else
        {
            if (transaction != null)
            {
                transaction.StatusId = MarketplaceTransactionStatuses.Ids.Failed;
                transaction.ErrorCode = result.ErrorCode;
                transaction.ErrorMessage = result.ErrorMessage;
                transaction.FailedAt = DateTime.UtcNow;
                transaction.CallbackReceivedAt = DateTime.UtcNow;
                transaction.UpdatedAt = DateTime.UtcNow;
            }

            if (reservationId > 0)
            {
                var reservation = await _reservationService.GetByIdAsync(reservationId);
                if (reservation != null)
                {
                    reservation.PaymentStatus = PaymentStatuses.Ids.Failed;
                    reservation.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            _logger.LogWarning("Payment failed for reservation {ReservationId}: {ErrorMessage}", reservationId, result.ErrorMessage);
            return (false, $"{_webBaseUrl}/Account/PaymentResult?status=failed&reservationId={reservationId}&error={Uri.EscapeDataString(result.ErrorMessage ?? "Error.PaymentFailed")}");
        }

        return (false, $"{_webBaseUrl}/Account/Reservations");
    }

    public async Task<object?> GetPaymentStatusAsync(int visitorId, int reservationId)
    {
        return await _reservationService.GetActiveReservations()
            .Where(r => r.Id == reservationId && r.VisitorId == visitorId)
            .Select(r => new
            {
                r.Id,
                r.PaymentId,
                PaymentStatus = r.PaymentStatus == PaymentStatuses.Ids.Pending ? "Pending" : r.PaymentStatus == PaymentStatuses.Ids.DepositPaid ? "DepositPaid" : r.PaymentStatus == PaymentStatuses.Ids.FullyPaid ? "FullyPaid" : r.PaymentStatus == PaymentStatuses.Ids.Failed ? "Failed" : r.PaymentStatus == PaymentStatuses.Ids.Refunded ? "Refunded" : "Unknown",
                PaymentStatusId = r.PaymentStatus,
                r.PaidAt,
                r.TotalPrice
            })
            .FirstOrDefaultAsync();
    }

    public async Task<object> GetPendingPaymentsAsync(int visitorId)
    {
        var reservations = await _reservationService.GetByVisitorId(visitorId)
            .Include(r => r.Tour)
            .Where(r => r.PaymentStatus == PaymentStatuses.Ids.Pending && r.Status != ReservationStatuses.Ids.Cancelled)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                r.TotalPrice,
                r.Date,
                StartTime = r.StartTime.ToString(@"hh\:mm"),
                r.CreatedAt
            })
            .ToListAsync();

        return new { reservations };
    }

    private async Task<(PaymentTransaction transaction, PaymentRequest request)> CreateMarketplacePaymentRequestAsync(
        Reservation reservation,
        decimal amount,
        int paymentTypeId,
        string productName,
        string callbackUrl,
        PaymentMethodSetting paymentMethod)
    {
        var company = reservation.Tour.Company;
        var commissionRate = company.PlatformCommissionRate > 0 ? company.PlatformCommissionRate : 12;
        var platformCommission = Math.Round(amount * commissionRate / 100, 2);
        var sellerReceivable = Math.Max(amount - platformCommission, 0);

        var transaction = new PaymentTransaction
        {
            ReservationId = reservation.Id,
            CompanyId = company.Id,
            VisitorId = reservation.VisitorId,
            TypeId = paymentTypeId,
            StatusId = MarketplaceTransactionStatuses.Ids.Initialized,
            Provider = paymentMethod.ProviderSystemName,
            PaymentMethodSystemName = paymentMethod.SystemName,
            Currency = "TRY",
            BuyerIp = "127.0.0.1",
            GrossAmount = amount,
            SellerReceivableAmount = sellerReceivable,
            PlatformCommissionAmount = platformCommission,
            PlatformCommissionRate = commissionRate
        };

        _marketplaceFinanceService.AddTransaction(transaction);
        await _unitOfWork.SaveChangesAsync();

        transaction.ConversationId = $"MKT-{transaction.Id}-{reservation.Id}-{DateTime.UtcNow.Ticks}";

        _marketplaceFinanceService.AddLineItem(new PaymentLineItem
        {
            PaymentTransactionId = transaction.Id,
            ReservationId = reservation.Id,
            CompanyId = company.Id,
            ItemId = $"TOUR-{reservation.Id}",
            ItemName = productName,
            SubMerchantKey = company.SubMerchantKey,
            ExternalSubMerchantId = company.SubMerchantExternalId,
            Price = amount,
            PaidPrice = amount,
            SubMerchantPrice = sellerReceivable,
            SubMerchantPayoutRate = 100 - commissionRate,
            SubMerchantPayoutAmount = sellerReceivable,
            MerchantPayoutAmount = platformCommission,
            PlatformCommissionAmount = platformCommission
        });

        await _unitOfWork.SaveChangesAsync();

        var splitEnabled = company.MarketplaceEnabled
            && company.SellerOnboardingStatusId == SellerOnboardingStatuses.Ids.Active
            && !string.IsNullOrWhiteSpace(company.SubMerchantKey);

        var paymentRequest = new PaymentRequest
        {
            ReservationId = reservation.Id,
            ConversationId = transaction.ConversationId,
            Amount = amount,
            PaymentTransactionTypeId = paymentTypeId,
            CustomerEmail = reservation.Visitor.Email,
            CustomerName = reservation.Visitor.FirstName,
            CustomerSurname = reservation.Visitor.LastName,
            CustomerPhone = reservation.Visitor.Phone ?? "",
            CustomerIp = "127.0.0.1",
            CustomerAddress = reservation.Visitor.Address ?? "",
            ProductName = productName,
            ProductCategory = "Tur",
            BasketItemId = $"TOUR-{reservation.Id}",
            PaymentMethodSystemName = paymentMethod.SystemName,
            ProviderSystemName = paymentMethod.ProviderSystemName,
            ProviderCredentials = BuildProviderCredentials(paymentMethod),
            SubMerchantKey = splitEnabled ? company.SubMerchantKey : null,
            SubMerchantExternalId = splitEnabled ? company.SubMerchantExternalId : null,
            SubMerchantPrice = splitEnabled ? sellerReceivable : null,
            PlatformCommissionRate = commissionRate,
            PlatformCommissionAmount = platformCommission,
            CallbackUrl = callbackUrl
        };

        return (transaction, paymentRequest);
    }

    private async Task<(bool success, PaymentMethodSetting? method, object errorResult, int statusCode)> ResolveCheckoutMethodAsync(string? paymentMethodSystemName)
    {
        var requestedSystemName = (paymentMethodSystemName ?? string.Empty).Trim().ToLowerInvariant();
        PaymentMethodSetting? method;

        if (string.IsNullOrWhiteSpace(requestedSystemName))
        {
            method = await _paymentMethodService.GetDefaultMethodAsync()
                ?? await _paymentMethodService.GetActiveMethods()
                    .Where(x => x.IsEnabled)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .FirstOrDefaultAsync();
        }
        else
        {
            method = await _paymentMethodService.GetActiveMethods()
                .FirstOrDefaultAsync(x => x.IsEnabled && x.SystemName == requestedSystemName);
        }

        if (method == null)
        {
            if (string.IsNullOrWhiteSpace(requestedSystemName) || requestedSystemName == "iyzico-card")
            {
                return (true, new PaymentMethodSetting
                {
                    SystemName = "iyzico-card",
                    DisplayName = "Kredi/Banka Karti",
                    ProviderSystemName = "iyzico",
                    ProviderDisplayName = "Iyzico",
                    IsEnabled = true,
                    IsDefault = true,
                    IsOnline = true,
                    SupportsMarketplaceSplit = true,
                    DisplayOrder = 1,
                    IconClass = "bi bi-credit-card-2-front"
                }, new { }, 200);
            }

            return (false, null, new
            {
                message = "Error.PaymentInitFailed",
                detail = "No enabled payment method found for this request."
            }, 400);
        }

        if (!method.IsOnline || !IsSupportedOnlineProvider(method.ProviderSystemName))
        {
            return (false, null, new
            {
                message = "Error.PaymentInitFailed",
                detail = "Selected payment method is not available for online checkout."
            }, 400);
        }

        return (true, method, new { }, 200);
    }

    private async Task<PaymentProviderCredentials?> ResolveProviderCredentialsAsync(string? paymentMethodSystemName, string? providerSystemName)
    {
        var method = !string.IsNullOrWhiteSpace(paymentMethodSystemName)
            ? await _paymentMethodService.GetActiveMethods()
                .FirstOrDefaultAsync(x => x.SystemName == paymentMethodSystemName)
            : null;

        if (method == null && !string.IsNullOrWhiteSpace(providerSystemName))
        {
            method = await _paymentMethodService.GetActiveMethods()
                .Where(x => x.ProviderSystemName == providerSystemName && x.IsEnabled)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.DisplayOrder)
                .FirstOrDefaultAsync();
        }

        return method == null ? null : BuildProviderCredentials(method);
    }

    private static PaymentProviderCredentials? BuildProviderCredentials(PaymentMethodSetting method)
    {
        if (string.IsNullOrWhiteSpace(method.ApiKey) ||
            string.IsNullOrWhiteSpace(method.SecretKey) ||
            string.IsNullOrWhiteSpace(method.BaseUrl))
        {
            return null;
        }

        return new PaymentProviderCredentials
        {
            ApiKey = method.ApiKey,
            SecretKey = method.SecretKey,
            BaseUrl = method.BaseUrl,
            IsSandbox = method.IsSandbox
        };
    }

    private static bool IsSupportedOnlineProvider(string providerSystemName)
        => providerSystemName.Equals("iyzico", StringComparison.OrdinalIgnoreCase);

    private void UpdateSuccessfulMarketplaceTransaction(PaymentTransaction? transaction, PaymentResult result, Reservation reservation)
    {
        if (transaction == null) return;

        var paidAt = DateTime.UtcNow;
        var paidAmount = result.PaidAmount ?? transaction.GrossAmount;

        transaction.StatusId = MarketplaceTransactionStatuses.Ids.Paid;
        transaction.PaymentId = result.PaymentId;
        transaction.PaidAmount = paidAmount;
        transaction.PaidAt = paidAt;
        transaction.CallbackReceivedAt = paidAt;
        transaction.UpdatedAt = paidAt;

        var providerItem = result.Items.FirstOrDefault();
        var lineItem = transaction.LineItems.FirstOrDefault();
        if (providerItem != null && lineItem != null)
        {
            lineItem.ProviderPaymentTransactionId = providerItem.PaymentTransactionId;
            lineItem.ProviderTransactionStatus = providerItem.TransactionStatus;
            lineItem.Price = providerItem.Price > 0 ? providerItem.Price : lineItem.Price;
            lineItem.PaidPrice = providerItem.PaidPrice > 0 ? providerItem.PaidPrice : lineItem.PaidPrice;
            lineItem.MerchantPayoutAmount = providerItem.MerchantPayoutAmount;
            lineItem.SubMerchantPayoutAmount = providerItem.SubMerchantPayoutAmount > 0 ? providerItem.SubMerchantPayoutAmount : lineItem.SubMerchantPayoutAmount;
            lineItem.IyziCommissionRateAmount = providerItem.IyziCommissionRateAmount;
            lineItem.IyziCommissionFee = providerItem.IyziCommissionFee;
            lineItem.BlockageRateAmountMerchant = providerItem.BlockageRateAmountMerchant;
            lineItem.BlockageRateAmountSubMerchant = providerItem.BlockageRateAmountSubMerchant;
            lineItem.BlockageResolvedDate = providerItem.BlockageResolvedDate;
            lineItem.UpdatedAt = paidAt;

            transaction.IyziCommissionRateAmount = providerItem.IyziCommissionRateAmount;
            transaction.IyziCommissionFee = providerItem.IyziCommissionFee;
        }

        if (transaction.LedgerEntries.Any(e => e.EntryTypeId == LedgerEntryTypes.Ids.CustomerCollection))
            return;

        var reference = string.IsNullOrWhiteSpace(result.PaymentId)
            ? transaction.ConversationId
            : result.PaymentId;
        var availableAt = paidAt.AddDays(transaction.Company.PayoutDelayDays);

        _marketplaceFinanceService.AddLedgerEntry(new MarketplaceLedgerEntry
        {
            PaymentTransactionId = transaction.Id,
            ReservationId = reservation.Id,
            CompanyId = transaction.CompanyId,
            EntryTypeId = LedgerEntryTypes.Ids.CustomerCollection,
            StatusId = LedgerEntryStatuses.Ids.Settled,
            Amount = paidAmount,
            Reference = reference,
            Description = "Musteri odemesi",
            OccurredAt = paidAt,
            SettledAt = paidAt
        });

        _marketplaceFinanceService.AddLedgerEntry(new MarketplaceLedgerEntry
        {
            PaymentTransactionId = transaction.Id,
            ReservationId = reservation.Id,
            CompanyId = transaction.CompanyId,
            EntryTypeId = LedgerEntryTypes.Ids.SellerReceivable,
            StatusId = LedgerEntryStatuses.Ids.Pending,
            Amount = transaction.SellerReceivableAmount,
            Reference = reference,
            Description = "Firma hak edisi",
            OccurredAt = paidAt,
            AvailableAt = availableAt
        });

        _marketplaceFinanceService.AddLedgerEntry(new MarketplaceLedgerEntry
        {
            PaymentTransactionId = transaction.Id,
            ReservationId = reservation.Id,
            CompanyId = transaction.CompanyId,
            EntryTypeId = LedgerEntryTypes.Ids.PlatformCommission,
            StatusId = LedgerEntryStatuses.Ids.Settled,
            Amount = transaction.PlatformCommissionAmount,
            Reference = reference,
            Description = "Platform komisyonu",
            OccurredAt = paidAt,
            SettledAt = paidAt
        });

        var providerFee = transaction.IyziCommissionFee + transaction.IyziCommissionRateAmount;
        if (providerFee > 0)
        {
            _marketplaceFinanceService.AddLedgerEntry(new MarketplaceLedgerEntry
            {
                PaymentTransactionId = transaction.Id,
                ReservationId = reservation.Id,
                CompanyId = transaction.CompanyId,
                EntryTypeId = LedgerEntryTypes.Ids.ProviderFee,
                StatusId = LedgerEntryStatuses.Ids.Settled,
                Amount = -providerFee,
                Reference = reference,
                Description = "Iyzico hizmet bedeli",
                OccurredAt = paidAt,
                SettledAt = paidAt
            });
        }
    }
}
