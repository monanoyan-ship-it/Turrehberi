using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IEmailAccountEntityService
{
    IQueryable<EmailAccount> GetActiveAccounts();
    Task<EmailAccount?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    void Add(EmailAccount account);
    void Update(EmailAccount account);
    Task ClearDefaultAsync();
    Task ClearDefaultExceptAsync(int id);
}
