namespace ErkanTatilPlani.Core.Factories.Reservations;

public interface IReservationPaymentFactory
{
    Task<(bool success, object result, int statusCode)> CreatePublicReservationAsync(int? visitorId, int tourId, string fullName, string email, string phone, int numberOfPeople, string? notes, string? address, DateTime? startDate, string customerIp);
    Task<(bool success, object result, int statusCode)> ProcessPaymentCallbackAsync(string token, int? reservationId);
    Task<object?> GetPaymentStatusAsync(int reservationId);
}
