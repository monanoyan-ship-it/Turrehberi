using ErkanTatilPlani.Core.Services;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

namespace ErkanTatilPlani.API.Services;

public class IyzicoPaymentService : IPaymentService
{
    private readonly PaymentSettings _settings;
    private readonly ILogger<IyzicoPaymentService> _logger;
    private readonly Iyzipay.Options _options;

    public IyzicoPaymentService(
        IOptions<PaymentSettings> settings,
        ILogger<IyzicoPaymentService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _options = new Iyzipay.Options
        {
            ApiKey = _settings.ApiKey,
            SecretKey = _settings.SecretKey,
            BaseUrl = _settings.BaseUrl
        };
    }

    public async Task<PaymentInitResult> InitializePaymentAsync(PaymentRequest request)
    {
        try
        {
            var checkoutFormRequest = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = string.IsNullOrWhiteSpace(request.ConversationId)
                    ? $"RES-{request.ReservationId}-{DateTime.UtcNow.Ticks}"
                    : request.ConversationId,
                Price = FormatDecimal(request.Amount),
                PaidPrice = FormatDecimal(request.Amount),
                Currency = Currency.TRY.ToString(),
                BasketId = $"B-{request.ReservationId}",
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = request.CallbackUrl,
                EnabledInstallments = new List<int> { 1, 2, 3, 6 }
            };

            // Alici Bilgileri
            var buyer = new Buyer
            {
                Id = $"BUYER-{request.ReservationId}",
                Name = request.CustomerName,
                Surname = request.CustomerSurname,
                Email = request.CustomerEmail,
                GsmNumber = FormatPhoneNumber(request.CustomerPhone),
                IdentityNumber = "11111111111", // TC Kimlik (zorunlu alan, gercek uygulamada alinmali)
                RegistrationAddress = string.IsNullOrEmpty(request.CustomerAddress) ? "Adres belirtilmedi" : request.CustomerAddress,
                City = request.CustomerCity,
                Country = request.CustomerCountry,
                Ip = request.CustomerIp
            };
            checkoutFormRequest.Buyer = buyer;

            // Fatura Adresi
            var billingAddress = new Address
            {
                ContactName = $"{request.CustomerName} {request.CustomerSurname}",
                City = request.CustomerCity,
                Country = request.CustomerCountry,
                Description = string.IsNullOrEmpty(request.CustomerAddress) ? "Adres belirtilmedi" : request.CustomerAddress
            };
            checkoutFormRequest.BillingAddress = billingAddress;
            checkoutFormRequest.ShippingAddress = billingAddress;

            // Sepet Urunleri
            var basketItem = new BasketItem
            {
                Id = string.IsNullOrWhiteSpace(request.BasketItemId) ? $"TOUR-{request.ReservationId}" : request.BasketItemId,
                Name = request.ProductName,
                Category1 = request.ProductCategory,
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Price = FormatDecimal(request.Amount)
            };

            if (!string.IsNullOrWhiteSpace(request.SubMerchantKey) && request.SubMerchantPrice.HasValue)
            {
                SetPropertyIfExists(basketItem, "SubMerchantKey", request.SubMerchantKey);
                SetPropertyIfExists(basketItem, "SubMerchantPrice", FormatDecimal(request.SubMerchantPrice.Value));
            }

            var basketItems = new List<BasketItem>
            {
                basketItem
            };
            checkoutFormRequest.BasketItems = basketItems;

            // Iyzico API'yi cagir
            var checkoutFormInitialize = await Task.Run(() =>
                CheckoutFormInitialize.Create(checkoutFormRequest, _options));

            if (checkoutFormInitialize.Status == "success")
            {
                _logger.LogInformation("Payment initialized for reservation {ReservationId}, Token: {Token}",
                    request.ReservationId, checkoutFormInitialize.Token);

                return new PaymentInitResult
                {
                    Success = true,
                    PaymentPageUrl = checkoutFormInitialize.PaymentPageUrl,
                    Token = checkoutFormInitialize.Token
                };
            }
            else
            {
                _logger.LogWarning("Payment initialization failed for reservation {ReservationId}: {ErrorCode} - {ErrorMessage}",
                    request.ReservationId, checkoutFormInitialize.ErrorCode, checkoutFormInitialize.ErrorMessage);

                return new PaymentInitResult
                {
                    Success = false,
                    ErrorCode = checkoutFormInitialize.ErrorCode,
                    ErrorMessage = checkoutFormInitialize.ErrorMessage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing payment for reservation {ReservationId}", request.ReservationId);
            return new PaymentInitResult
            {
                Success = false,
                ErrorMessage = "Odeme baslatilirken bir hata olustu"
            };
        }
    }

    public async Task<PaymentResult> ProcessCallbackAsync(string token)
    {
        try
        {
            var request = new RetrieveCheckoutFormRequest
            {
                Locale = Locale.TR.ToString(),
                Token = token
            };

            var checkoutForm = await Task.Run(() =>
                CheckoutForm.Retrieve(request, _options));

            // ConversationId'den ReservationId'yi cikar
            var conversationId = checkoutForm.ConversationId ?? "";
            var reservationId = 0;
            if (conversationId.StartsWith("RES-"))
            {
                var parts = conversationId.Split('-');
                if (parts.Length >= 2)
                {
                    int.TryParse(parts[1], out reservationId);
                }
            }

            if (checkoutForm.Status == "success" && checkoutForm.PaymentStatus == "SUCCESS")
            {
                _logger.LogInformation("Payment successful for token {Token}, PaymentId: {PaymentId}",
                    token, checkoutForm.PaymentId);

                return new PaymentResult
                {
                    Success = true,
                    PaymentId = checkoutForm.PaymentId,
                    ConversationId = checkoutForm.ConversationId,
                    PaidAmount = decimal.TryParse(checkoutForm.PaidPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var paid) ? paid : 0,
                    ReservationId = reservationId,
                    Items = ExtractPaymentItems(checkoutForm)
                };
            }
            else
            {
                _logger.LogWarning("Payment failed for token {Token}: {ErrorCode} - {ErrorMessage}",
                    token, checkoutForm.ErrorCode, checkoutForm.ErrorMessage);

                return new PaymentResult
                {
                    Success = false,
                    ErrorCode = checkoutForm.ErrorCode,
                    ErrorMessage = checkoutForm.ErrorMessage ?? "Odeme basarisiz",
                    ReservationId = reservationId
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment callback for token {Token}", token);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = "Odeme sonucu islenirken bir hata olustu"
            };
        }
    }

    public async Task<PaymentStatus> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            var request = new RetrievePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                PaymentId = paymentId
            };

            var payment = await Task.Run(() =>
                Payment.Retrieve(request, _options));

            if (payment.Status == "success")
            {
                var status = payment.PaymentStatus switch
                {
                    "SUCCESS" => "Paid",
                    "FAILURE" => "Failed",
                    "INIT_THREEDS" or "CALLBACK_THREEDS" => "Pending",
                    _ => "Unknown"
                };

                return new PaymentStatus
                {
                    Success = true,
                    Status = status,
                    PaymentId = payment.PaymentId,
                    PaidAmount = decimal.TryParse(payment.PaidPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var paid) ? paid : 0
                };
            }
            else
            {
                return new PaymentStatus
                {
                    Success = false,
                    Status = "Unknown",
                    ErrorMessage = payment.ErrorMessage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for {PaymentId}", paymentId);
            return new PaymentStatus
            {
                Success = false,
                Status = "Error",
                ErrorMessage = "Odeme durumu sorgulanirken bir hata olustu"
            };
        }
    }

    private string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "+905000000000";

        // Sadece rakamlari al
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // Turkiye numarasi formatina cevir
        if (digits.StartsWith("0") && digits.Length == 11)
        {
            return "+9" + digits;
        }
        else if (digits.StartsWith("90") && digits.Length == 12)
        {
            return "+" + digits;
        }
        else if (digits.Length == 10)
        {
            return "+90" + digits;
        }

        return "+90" + digits;
    }

    private static string FormatDecimal(decimal value)
        => value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    private static void SetPropertyIfExists(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property?.CanWrite == true)
        {
            property.SetValue(target, value);
        }
    }

    private static List<MarketplacePaymentItemResult> ExtractPaymentItems(object checkoutForm)
    {
        var items = new List<MarketplacePaymentItemResult>();
        var itemTransactions = checkoutForm.GetType().GetProperty("ItemTransactions")?.GetValue(checkoutForm) as System.Collections.IEnumerable;
        if (itemTransactions == null) return items;

        foreach (var item in itemTransactions)
        {
            items.Add(new MarketplacePaymentItemResult
            {
                ItemId = ReadString(item, "ItemId"),
                PaymentTransactionId = ReadString(item, "PaymentTransactionId"),
                TransactionStatus = ReadInt(item, "TransactionStatus"),
                Price = ReadDecimal(item, "Price"),
                PaidPrice = ReadDecimal(item, "PaidPrice"),
                MerchantPayoutAmount = ReadDecimal(item, "MerchantPayoutAmount"),
                SubMerchantPayoutAmount = ReadDecimal(item, "SubMerchantPayoutAmount"),
                IyziCommissionRateAmount = ReadDecimal(item, "IyziCommissionRateAmount"),
                IyziCommissionFee = ReadDecimal(item, "IyziCommissionFee"),
                BlockageRateAmountMerchant = ReadDecimal(item, "BlockageRateAmountMerchant"),
                BlockageRateAmountSubMerchant = ReadDecimal(item, "BlockageRateAmountSubMerchant"),
                BlockageResolvedDate = ReadDate(item, "BlockageResolvedDate")
            });
        }

        return items;
    }

    private static string ReadString(object target, string propertyName)
        => target.GetType().GetProperty(propertyName)?.GetValue(target)?.ToString() ?? string.Empty;

    private static int ReadInt(object target, string propertyName)
    {
        var value = target.GetType().GetProperty(propertyName)?.GetValue(target)?.ToString();
        return int.TryParse(value, out var number) ? number : 0;
    }

    private static decimal ReadDecimal(object target, string propertyName)
    {
        var value = target.GetType().GetProperty(propertyName)?.GetValue(target)?.ToString();
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var number)
            ? number
            : 0;
    }

    private static DateTime? ReadDate(object target, string propertyName)
    {
        var value = target.GetType().GetProperty(propertyName)?.GetValue(target)?.ToString();
        return DateTime.TryParse(value, out var date) ? date : null;
    }
}
