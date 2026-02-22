using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Guides;

public interface IGuideFactory
{
    Task<(bool success, object result, int statusCode)> GetCompanyGuidesAsync(int visitorId);
    Task<(bool success, object result, int statusCode)> GetGuideByIdAsync(int visitorId, int guideId);
    Task<(bool success, object result, int statusCode)> CreateGuideAsync(int visitorId, Guide guide);
    Task<(bool success, object result, int statusCode)> UpdateGuideAsync(int visitorId, int guideId, Guide guide);
    Task<(bool success, object result, int statusCode)> DeleteGuideAsync(int visitorId, int guideId);
    Task<(bool success, object result, int statusCode)> AssignGuideToDateAsync(int visitorId, int guideId, int tourDateId, string? notes);
    Task<(bool success, object result, int statusCode)> RemoveAssignmentAsync(int visitorId, int assignmentId);
    Task<(bool success, object result, int statusCode)> GetGuideAvailabilityAsync(int visitorId, int guideId, int year, int month);
    Task<(bool success, object result, int statusCode)> GetGuidePerformanceAsync(int visitorId, int guideId);
    Task<(bool success, object result, int statusCode)> UploadGuidePhotoAsync(int visitorId, int guideId, Stream fileStream, string fileName);
    Task<(bool success, object result, int statusCode)> DeleteGuidePhotoAsync(int visitorId, int guideId);
}
