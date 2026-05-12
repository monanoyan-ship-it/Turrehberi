namespace ErkanTatilPlani.Core.Services;

public interface IMarketplacePaymentService
{
    Task<SellerOnboardingResult> CreateOrUpdateSubMerchantAsync(SellerOnboardingRequest request);
    Task<MarketplaceProviderRefundResult> RefundAsync(MarketplaceProviderRefundRequest request);
}

public class SellerOnboardingRequest
{
    public int CompanyId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public int SellerLegalTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LegalCompanyTitle { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = "11111111111";
    public string Iban { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactSurname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool HasExistingSubMerchantKey { get; set; }
    public string SubMerchantKey { get; set; } = string.Empty;
}

public class SellerOnboardingResult
{
    public bool Success { get; set; }
    public string? SubMerchantKey { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class MarketplaceProviderRefundRequest
{
    public string PaymentTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string ConversationId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
}

public class MarketplaceProviderRefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
