namespace ErkanTatilPlani.Core.Factories.Reservations;

public interface IVisitorReservationFactory
{
    Task<object> GetVisitorReservationsAsync(int visitorId);
    Task<object?> GetVisitorReservationDetailAsync(int visitorId, int reservationId);
    Task<(bool success, object result, int statusCode)> CancelVisitorReservationAsync(int visitorId, int reservationId);
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetMyReservationsAsync(int visitorId, string? status);
    Task<(bool success, object? result, int statusCode)> UpdateMyReservationStatusAsync(int visitorId, int reservationId, string status, string? rejectionReason);
    Task<(bool success, string message)> ChangeDateAsync(int visitorId, int reservationId, DateTime newStartDate);
    Task<(bool success, string message)> UpdatePhotoLinkAsync(int reservationId, string photoLink);
}
