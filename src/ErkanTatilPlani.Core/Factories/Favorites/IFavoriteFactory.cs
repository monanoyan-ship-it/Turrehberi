namespace ErkanTatilPlani.Core.Factories.Favorites;

public interface IFavoriteFactory
{
    Task<object> GetFavoritesAsync(int visitorId);
    Task<bool> CheckFavoriteAsync(int visitorId, int tourId);
    Task<List<int>> CheckMultipleFavoritesAsync(int visitorId, int[] tourIds);
    Task<(bool success, string message)> AddFavoriteAsync(int visitorId, int tourId);
    Task<(bool success, string message)> RemoveFavoriteAsync(int visitorId, int tourId);
    Task<(bool isFavorite, string message, bool tourNotFound)> ToggleFavoriteAsync(int visitorId, int tourId);
}
