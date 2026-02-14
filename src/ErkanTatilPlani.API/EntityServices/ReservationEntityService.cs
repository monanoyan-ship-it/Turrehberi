using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class ReservationEntityService : IReservationEntityService
{
    private readonly AppDbContext _context;

    public ReservationEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Reservation> GetActiveReservations()
        => _context.Reservations.Where(r => r.IsActive);

    public async Task<Reservation?> GetByIdAsync(int id)
        => await _context.Reservations.FindAsync(id);

    public async Task<Reservation?> GetByIdWithDetailsAsync(int id)
        => await _context.Reservations
            .Include(r => r.Tour).ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Reservation?> GetByPaymentTokenAsync(string token)
        => await _context.Reservations
            .Include(r => r.Tour).ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .FirstOrDefaultAsync(r => r.PaymentToken == token);

    public IQueryable<Reservation> GetByVisitorId(int visitorId)
        => _context.Reservations.Where(r => r.VisitorId == visitorId && r.IsActive);

    public IQueryable<Reservation> GetByTourIds(IEnumerable<int> tourIds)
        => _context.Reservations.Where(r => tourIds.Contains(r.TourId) && r.IsActive);

    public void Add(Reservation reservation) => _context.Reservations.Add(reservation);

    public void Remove(Reservation reservation) => _context.Reservations.Remove(reservation);

    public void Update(Reservation reservation) => _context.Entry(reservation).State = EntityState.Modified;
}
