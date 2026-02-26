namespace ErkanTatilPlani.Core.Factories.Recommendations;

public interface ITourRecommendationFactory
{
    Task<object> GetRecommendationsAsync(int visitorId, int limit = 5);
    Task<object> GetRecommendationsByReservationAsync(int reservationId, int limit = 5);
}
