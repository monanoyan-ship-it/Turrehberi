using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IVisitorEntityService
{
    Task<IEnumerable<Visitor>> GetActiveVisitorsAsync();
    Task<Visitor?> GetByIdAsync(int id);
    Task<Visitor?> GetByIdWithCompanyAsync(int id);
    Task<Visitor?> GetByEmailAsync(string email);
    Task<Visitor?> GetActiveByEmailAsync(string email);
    Task<Visitor?> GetActiveByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<Visitor?> GetByReferralCodeAsync(string referralCode);
    void Add(Visitor visitor);
    void Update(Visitor visitor);
}
