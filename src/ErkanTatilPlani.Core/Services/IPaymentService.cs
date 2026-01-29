namespace ErkanTatilPlani.Core.Services;

public interface IPaymentService
{
    Task<PaymentInitResult> InitializePaymentAsync(PaymentRequest request);
    Task<PaymentResult> ProcessCallbackAsync(string token);
    Task<PaymentStatus> GetPaymentStatusAsync(string paymentId);
}

public class PaymentRequest
{
    public int ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";

    // Musteri Bilgileri
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerSurname { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerIp { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = "Istanbul";
    public string CustomerCountry { get; set; } = "Turkey";
    public string CustomerAddress { get; set; } = string.Empty;

    // Urun Bilgileri
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = "Tur";

    // Callback URL'leri
    public string CallbackUrl { get; set; } = string.Empty;
}

public class PaymentInitResult
{
    public bool Success { get; set; }
    public string? PaymentPageUrl { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? PaymentId { get; set; }
    public string? ConversationId { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public int ReservationId { get; set; }
}

public class PaymentStatus
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Paid, Failed, Refunded
    public string? PaymentId { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com"; // sandbox or production
    public bool IsSandbox { get; set; } = true;
}
