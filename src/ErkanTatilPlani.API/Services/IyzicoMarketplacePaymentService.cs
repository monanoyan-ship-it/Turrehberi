using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Services;
using Iyzipay;
using Microsoft.Extensions.Options;

namespace ErkanTatilPlani.API.Services;

public class IyzicoMarketplacePaymentService : IMarketplacePaymentService
{
    private readonly Iyzipay.Options _options;
    private readonly ILogger<IyzicoMarketplacePaymentService> _logger;

    public IyzicoMarketplacePaymentService(
        IOptions<PaymentSettings> settings,
        ILogger<IyzicoMarketplacePaymentService> logger)
    {
        _logger = logger;
        _options = new Iyzipay.Options
        {
            ApiKey = settings.Value.ApiKey,
            SecretKey = settings.Value.SecretKey,
            BaseUrl = settings.Value.BaseUrl
        };
    }

    public async Task<SellerOnboardingResult> CreateOrUpdateSubMerchantAsync(SellerOnboardingRequest request)
    {
        try
        {
            var requestTypeName = request.HasExistingSubMerchantKey
                ? "Iyzipay.Request.UpdateSubMerchantRequest, Iyzipay"
                : "Iyzipay.Request.CreateSubMerchantRequest, Iyzipay";
            var apiType = Type.GetType("Iyzipay.Model.SubMerchant, Iyzipay");
            var requestType = Type.GetType(requestTypeName);

            if (apiType == null || requestType == null)
            {
                return new SellerOnboardingResult
                {
                    Success = false,
                    ErrorMessage = "Iyzico marketplace SDK tipi bulunamadi"
                };
            }

            var providerRequest = Activator.CreateInstance(requestType)!;
            SetProperty(providerRequest, "Locale", "tr");
            SetProperty(providerRequest, "ConversationId", $"SELLER-{request.CompanyId}-{DateTime.UtcNow.Ticks}");
            SetProperty(providerRequest, "SubMerchantExternalId", request.ExternalId);
            SetProperty(providerRequest, "SubMerchantType", GetSubMerchantType(request.SellerLegalTypeId));
            SetProperty(providerRequest, "Address", string.IsNullOrWhiteSpace(request.Address) ? "Adres belirtilmedi" : request.Address);
            SetProperty(providerRequest, "ContactName", string.IsNullOrWhiteSpace(request.ContactName) ? request.Name : request.ContactName);
            SetProperty(providerRequest, "ContactSurname", string.IsNullOrWhiteSpace(request.ContactSurname) ? request.Name : request.ContactSurname);
            SetProperty(providerRequest, "Email", request.Email);
            SetProperty(providerRequest, "GsmNumber", FormatPhoneNumber(request.Phone));
            SetProperty(providerRequest, "Name", request.Name);
            SetProperty(providerRequest, "Iban", request.Iban);
            SetProperty(providerRequest, "Currency", "TRY");
            SetProperty(providerRequest, "IdentityNumber", string.IsNullOrWhiteSpace(request.IdentityNumber) ? "11111111111" : request.IdentityNumber);
            SetProperty(providerRequest, "TaxNumber", request.TaxNumber);
            SetProperty(providerRequest, "TaxOffice", request.TaxOffice);
            SetProperty(providerRequest, "LegalCompanyTitle", string.IsNullOrWhiteSpace(request.LegalCompanyTitle) ? request.Name : request.LegalCompanyTitle);
            SetProperty(providerRequest, "SubMerchantKey", request.SubMerchantKey);

            var methodName = request.HasExistingSubMerchantKey ? "Update" : "Create";
            var method = apiType.GetMethod(methodName, new[] { requestType, typeof(Iyzipay.Options) });
            if (method == null)
            {
                return new SellerOnboardingResult
                {
                    Success = false,
                    ErrorMessage = "Iyzico marketplace metodu bulunamadi"
                };
            }

            var response = await Task.Run(() => method.Invoke(null, new[] { providerRequest, _options }));
            var status = ReadString(response, "Status");
            var subMerchantKey = ReadString(response, "SubMerchantKey");

            return new SellerOnboardingResult
            {
                Success = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase),
                SubMerchantKey = subMerchantKey,
                ErrorCode = ReadString(response, "ErrorCode"),
                ErrorMessage = ReadString(response, "ErrorMessage")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iyzico submerchant onboarding failed for company {CompanyId}", request.CompanyId);
            return new SellerOnboardingResult
            {
                Success = false,
                ErrorMessage = "Alt uye kaydi sirasinda hata olustu"
            };
        }
    }

    public async Task<MarketplaceProviderRefundResult> RefundAsync(MarketplaceProviderRefundRequest request)
    {
        try
        {
            var requestType = Type.GetType("Iyzipay.Request.CreateRefundRequest, Iyzipay");
            var apiType = Type.GetType("Iyzipay.Model.Refund, Iyzipay");

            if (requestType == null || apiType == null)
            {
                return new MarketplaceProviderRefundResult
                {
                    Success = false,
                    ErrorMessage = "Iyzico refund SDK tipi bulunamadi"
                };
            }

            var providerRequest = Activator.CreateInstance(requestType)!;
            SetProperty(providerRequest, "Locale", "tr");
            SetProperty(providerRequest, "ConversationId", string.IsNullOrWhiteSpace(request.ConversationId) ? $"REF-{DateTime.UtcNow.Ticks}" : request.ConversationId);
            SetProperty(providerRequest, "PaymentTransactionId", request.PaymentTransactionId);
            SetProperty(providerRequest, "Price", FormatDecimal(request.Amount));
            SetProperty(providerRequest, "Currency", request.Currency);
            SetProperty(providerRequest, "Ip", request.IpAddress);

            var method = apiType.GetMethod("Create", new[] { requestType, typeof(Iyzipay.Options) });
            if (method == null)
            {
                return new MarketplaceProviderRefundResult
                {
                    Success = false,
                    ErrorMessage = "Iyzico refund metodu bulunamadi"
                };
            }

            var response = await Task.Run(() => method.Invoke(null, new[] { providerRequest, _options }));
            var status = ReadString(response, "Status");

            return new MarketplaceProviderRefundResult
            {
                Success = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase),
                RefundId = ReadString(response, "PaymentId"),
                ErrorCode = ReadString(response, "ErrorCode"),
                ErrorMessage = ReadString(response, "ErrorMessage")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iyzico refund failed for provider transaction {PaymentTransactionId}", request.PaymentTransactionId);
            return new MarketplaceProviderRefundResult
            {
                Success = false,
                ErrorMessage = "Iade saglayici tarafinda islenemedi"
            };
        }
    }

    private static string GetSubMerchantType(int sellerLegalTypeId)
        => sellerLegalTypeId switch
        {
            SellerLegalTypes.Ids.SoleProprietorship => "PRIVATE_COMPANY",
            SellerLegalTypes.Ids.Individual => "PERSONAL",
            _ => "LIMITED_OR_JOINT_STOCK_COMPANY"
        };

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property?.CanWrite == true)
        {
            property.SetValue(target, value);
        }
    }

    private static string ReadString(object? target, string propertyName)
        => target?.GetType().GetProperty(propertyName)?.GetValue(target)?.ToString() ?? string.Empty;

    private static string FormatDecimal(decimal value)
        => value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "+905000000000";
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0") && digits.Length == 11) return "+9" + digits;
        if (digits.StartsWith("90") && digits.Length == 12) return "+" + digits;
        if (digits.Length == 10) return "+90" + digits;
        return "+90" + digits;
    }
}
