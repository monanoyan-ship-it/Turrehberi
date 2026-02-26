using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class VisitorEntityService : IVisitorEntityService
{
    private readonly AppDbContext _context;

    public VisitorEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Visitor>> GetActiveVisitorsAsync()
        => await _context.Visitors.Where(v => v.IsActive).ToListAsync();

    public async Task<Visitor?> GetByIdAsync(int id)
        => await _context.Visitors.FindAsync(id);

    public async Task<Visitor?> GetByIdWithCompanyAsync(int id)
        => await _context.Visitors.Include(v => v.Company).FirstOrDefaultAsync(v => v.Id == id && v.IsActive);

    public async Task<Visitor?> GetByEmailAsync(string email)
        => await _context.Visitors.FirstOrDefaultAsync(v => v.Email == email);

    public async Task<Visitor?> GetActiveByEmailAsync(string email)
        => await _context.Visitors.Include(v => v.Company).FirstOrDefaultAsync(v => v.Email == email && v.IsActive);

    public async Task<Visitor?> GetActiveByIdAsync(int id)
        => await _context.Visitors.FirstOrDefaultAsync(v => v.Id == id && v.IsActive);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Visitors.AnyAsync(v => v.Email == email);

    public async Task<Visitor?> GetByReferralCodeAsync(string referralCode)
        => await _context.Visitors.FirstOrDefaultAsync(v => v.ReferralCode == referralCode && v.IsActive);

    public void Add(Visitor visitor) => _context.Visitors.Add(visitor);

    public void Update(Visitor visitor) => _context.Entry(visitor).State = EntityState.Modified;
}
