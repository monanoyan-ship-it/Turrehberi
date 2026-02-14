namespace ErkanTatilPlani.Core.Factories.Payments;

public interface IPaymentFactory
{
    Task<(bool success, object result, int statusCode)> InitializePaymentAsync(int visitorId, int reservationId, string scheme, string host);
    Task<(bool success, object result, int statusCode)> InitializeRemainingPaymentAsync(int visitorId, int reservationId, string scheme, string host);
    Task<(bool success, string? redirectUrl)> ProcessCallbackAsync(string token);
    Task<object?> GetPaymentStatusAsync(int visitorId, int reservationId);
    Task<object> GetPendingPaymentsAsync(int visitorId);
}
