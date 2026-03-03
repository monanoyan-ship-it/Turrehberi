namespace ErkanTatilPlani.Core.Factories.Tours;

/// <summary>
/// Tur paketi olusturucu - kullanici kendi paketini yapar (Faz 15.6)
/// </summary>
public interface ITourPackageFactory
{
    Task<(bool success, object result, int statusCode)> GetMyPackagesAsync(int visitorId);
    Task<(bool success, object result, int statusCode)> GetPublicPackagesAsync(int page = 1, int pageSize = 20);
    Task<(bool success, object result, int statusCode)> GetPackageByIdAsync(int packageId);
    Task<(bool success, object result, int statusCode)> CreatePackageAsync(int visitorId, string name, string? description, bool isPublic, List<PackageItemInput> items);
    Task<(bool success, object result, int statusCode)> DeletePackageAsync(int visitorId, int packageId);
}

public class PackageItemInput
{
    public int TourId { get; set; }
    public int NumberOfPeople { get; set; } = 1;
}
