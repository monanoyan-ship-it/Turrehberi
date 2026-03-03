namespace ErkanTatilPlani.Core.Factories.Tours;

/// <summary>
/// Tur sonrasi dijital icerik - firma tarafindan yonetilir (Faz 15.7)
/// </summary>
public interface ITourDigitalContentFactory
{
    Task<(bool success, object result, int statusCode)> GetTourContentsAsync(int tourId, int? visitorId);
    Task<(bool success, object result, int statusCode)> ManageContentAsync(int visitorId, int tourId, int? contentId, string title, string? description, int contentTypeId, string? fileUrl, string? content, int sortOrder, bool requiresReservation);
    Task<(bool success, object result, int statusCode)> DeleteContentAsync(int visitorId, int contentId);
}
