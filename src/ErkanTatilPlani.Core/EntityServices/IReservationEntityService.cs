using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IReservationEntityService
{
    IQueryable<Reservation> GetActiveReservations();
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation?> GetByIdWithDetailsAsync(int id);
    Task<Reservation?> GetByPaymentTokenAsync(string token);
    IQueryable<Reservation> GetByVisitorId(int visitorId);
    IQueryable<Reservation> GetByTourIds(IEnumerable<int> tourIds);
    void Add(Reservation reservation);
    void Remove(Reservation reservation);
    void Update(Reservation reservation);
}
