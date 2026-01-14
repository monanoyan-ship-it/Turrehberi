using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation?> CreateAsync(Reservation reservation);
    Task<bool> UpdateAsync(int id, Reservation reservation);
    Task<bool> UpdateStatusAsync(int id, ReservationStatus status);
}
