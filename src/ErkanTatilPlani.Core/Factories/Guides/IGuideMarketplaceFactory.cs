namespace ErkanTatilPlani.Core.Factories.Guides;

/// <summary>
/// Bagimsiz rehber pazaryeri - herkese acik rehber katalogu (Faz 15.3)
/// </summary>
public interface IGuideMarketplaceFactory
{
    Task<object> GetPublicGuidesAsync(string? language = null, string? destination = null, int page = 1, int pageSize = 20);
    Task<object> GetGuideProfileAsync(int guideId);
}
