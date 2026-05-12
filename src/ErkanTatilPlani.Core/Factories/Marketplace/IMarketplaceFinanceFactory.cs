namespace ErkanTatilPlani.Core.Factories.Marketplace;

public interface IMarketplaceFinanceFactory
{
    Task<object> GetAdminOverviewAsync();
    Task<object> GetAdminSellersAsync();
    Task<object> GetAdminTransactionsAsync(int? companyId = null, int? statusId = null);
    Task<object> GetAdminRefundsAsync(int? companyId = null, int? statusId = null);
    Task<object> GetAdminPayoutsAsync(int? companyId = null, int? statusId = null);
    Task<(bool success, object result, int statusCode)> UpdateSellerSettingsAsync(int companyId, MarketplaceSellerSettingsRequest request);
    Task<(bool success, object result, int statusCode)> OnboardSellerAsync(int companyId);
    Task<(bool success, object result, int statusCode)> CreateRefundAsync(int transactionId, CreateMarketplaceRefundRequest request, int? processedById);
    Task<(bool success, object result, int statusCode)> CreatePayoutBatchAsync(int companyId, CreatePayoutBatchRequest request, int? approvedById);
    Task<(bool success, object result, int statusCode)> MarkPayoutPaidAsync(int payoutId, MarkPayoutPaidRequest request, int? approvedById);
    Task<(bool success, object result, int statusCode)> GetCompanyOverviewAsync(int visitorId);
    Task<(bool success, object result, int statusCode)> UpdateMySellerSettingsAsync(int visitorId, MarketplaceSellerSettingsRequest request);
    Task<(bool success, object result, int statusCode)> OnboardMySellerAsync(int visitorId);
}

public class MarketplaceSellerSettingsRequest
{
    public int SellerLegalTypeId { get; set; }
    public bool MarketplaceEnabled { get; set; }
    public decimal PlatformCommissionRate { get; set; }
    public int PayoutDelayDays { get; set; }
    public string LegalCompanyTitle { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactSurname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class CreateMarketplaceRefundRequest
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreatePayoutBatchRequest
{
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public string? Notes { get; set; }
}

public class MarkPayoutPaidRequest
{
    public string? BankReference { get; set; }
    public string? Notes { get; set; }
}
