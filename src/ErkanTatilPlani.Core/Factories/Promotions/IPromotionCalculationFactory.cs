namespace ErkanTatilPlani.Core.Factories.Promotions;

public interface IPromotionCalculationFactory
{
    Task<PriceCalculationResult> CalculatePriceAsync(int tourId, int numberOfPeople, DateTime? tourStartDate = null, string? couponCode = null, int? visitorId = null);
    Task<(bool valid, decimal discountAmount, string? errorMessage)> ValidateCouponAsync(string code, int tourId, int numberOfPeople, decimal totalPrice, int? visitorId = null);
    Task<object> GetTourPromotionBadgesAsync(int tourId, DateTime? tourStartDate = null);
    Task<object> GetActiveFlashSalesAsync();
    Task<object> GetLastMinuteDealsAsync();
}
