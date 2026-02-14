using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Reservations;

public interface IReservationCrudFactory
{
    Task<object> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation> CreateAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task<bool> UpdateStatusAsync(int id, ReservationStatus status);
}
