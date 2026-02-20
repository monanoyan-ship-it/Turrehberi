using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Promotions;

public interface IPromotionCrudFactory
{
    Task<(bool success, object result, int statusCode)> GetCompanyPromotionsAsync(int visitorId, int? typeFilter = null, int? statusFilter = null);
    Task<(bool success, object result, int statusCode)> GetPromotionByIdAsync(int visitorId, int promotionId);
    Task<(bool success, object result, int statusCode)> CreatePromotionAsync(int visitorId, Promotion promotion);
    Task<(bool success, object result, int statusCode)> UpdatePromotionAsync(int visitorId, int promotionId, Promotion promotion);
    Task<(bool success, object result, int statusCode)> DeletePromotionAsync(int visitorId, int promotionId);
    Task<(bool success, object result, int statusCode)> ToggleStatusAsync(int visitorId, int promotionId);
    Task<(bool success, object result, int statusCode)> GetAllPromotionsAsync(int? companyId = null, int? typeFilter = null, int? statusFilter = null);
}
