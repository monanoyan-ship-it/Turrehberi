namespace ErkanTatilPlani.Core.Factories.Loyalty;

public interface ILoyaltyFactory
{
    Task<object> GetLoyaltyDashboardAsync(int visitorId);
    Task RecalculateTierAsync(int visitorId);
    Task EarnCreditsAsync(int visitorId, int reservationId, decimal totalPrice);
    Task<(bool success, string? errorMessage)> SpendCreditsAsync(int visitorId, decimal amount, int reservationId);
    Task ExpireCreditsAsync();
    Task<object> GetCreditHistoryAsync(int visitorId, int page = 1, int pageSize = 20);
}
