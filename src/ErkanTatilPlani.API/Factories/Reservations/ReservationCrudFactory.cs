using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Reservations;

public class ReservationCrudFactory : IReservationCrudFactory
{
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationCrudFactory(IReservationEntityService reservationService, IUnitOfWork unitOfWork)
    {
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetAllAsync()
    {
        return await _reservationService.GetActiveReservations()
            .Include(r => r.Tour)
                .ThenInclude(t => t.Company)
            .Include(r => r.Visitor)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _reservationService.GetByIdWithDetailsAsync(id);
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        _reservationService.Add(reservation);
        await _unitOfWork.SaveChangesAsync();
        return reservation;
    }

    public async Task UpdateAsync(Reservation reservation)
    {
        reservation.UpdatedAt = DateTime.UtcNow;
        _reservationService.Update(reservation);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, int statusId)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null) return false;

        reservation.Status = statusId;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
