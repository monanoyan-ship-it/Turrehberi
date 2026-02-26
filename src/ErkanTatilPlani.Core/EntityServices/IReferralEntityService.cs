using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IReferralEntityService
{
    IQueryable<Referral> GetByReferrerId(int visitorId);
    Task<Referral?> GetByReferredVisitorIdAsync(int visitorId);
    Task<Referral?> GetByCodeAndReferredAsync(string code, int referredVisitorId);
    void Add(Referral referral);
    void Update(Referral referral);
}
