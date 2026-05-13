namespace ErkanTatilPlani.Core.Entities;

public class PaymentMethodSetting : BaseEntity
{
    public string SystemName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string ProviderSystemName { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public bool IsOnline { get; set; } = true;
    public bool SupportsMarketplaceSplit { get; set; }
    public int DisplayOrder { get; set; }

    public string IconClass { get; set; } = "bi bi-credit-card-2-front";

    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;

    public string ExtraSettingsJson { get; set; } = string.Empty;
}
