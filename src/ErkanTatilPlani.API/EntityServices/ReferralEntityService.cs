using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class ReferralEntityService : IReferralEntityService
{
    private readonly AppDbContext _context;

    public ReferralEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Referral> GetByReferrerId(int visitorId)
        => _context.Referrals.Where(r => r.ReferrerVisitorId == visitorId && r.IsActive);

    public async Task<Referral?> GetByReferredVisitorIdAsync(int visitorId)
        => await _context.Referrals.FirstOrDefaultAsync(r => r.ReferredVisitorId == visitorId && r.IsActive);

    public async Task<Referral?> GetByCodeAndReferredAsync(string code, int referredVisitorId)
        => await _context.Referrals.FirstOrDefaultAsync(r =>
            r.ReferralCode == code && r.ReferredVisitorId == referredVisitorId && r.IsActive);

    public void Add(Referral referral) => _context.Referrals.Add(referral);
    public void Update(Referral referral) => _context.Entry(referral).State = EntityState.Modified;
}
