using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class AbandonedCartEntityService : IAbandonedCartEntityService
{
    private readonly AppDbContext _context;

    public AbandonedCartEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<AbandonedCart> GetUnrecoveredCarts(DateTime olderThan)
        => _context.AbandonedCarts.Where(c =>
            c.IsActive &&
            !c.Recovered &&
            !c.EmailSent &&
            c.CreatedAt <= olderThan);

    public async Task<AbandonedCart?> GetByEmailAndTourAsync(string email, int tourId)
        => await _context.AbandonedCarts.FirstOrDefaultAsync(c =>
            c.Email == email && c.TourId == tourId && c.IsActive && !c.Recovered);

    public async Task<AbandonedCart?> GetByVisitorAndTourAsync(int visitorId, int tourId)
        => await _context.AbandonedCarts.FirstOrDefaultAsync(c =>
            c.VisitorId == visitorId && c.TourId == tourId && c.IsActive && !c.Recovered);

    public void Add(AbandonedCart cart) => _context.AbandonedCarts.Add(cart);
    public void Update(AbandonedCart cart) => _context.Entry(cart).State = EntityState.Modified;
}
