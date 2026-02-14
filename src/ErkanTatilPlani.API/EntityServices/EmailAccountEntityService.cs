using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class EmailAccountEntityService : IEmailAccountEntityService
{
    private readonly AppDbContext _context;

    public EmailAccountEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<EmailAccount> GetActiveAccounts()
        => _context.EmailAccounts.Where(a => a.IsActive);

    public async Task<EmailAccount?> GetByIdAsync(int id)
        => await _context.EmailAccounts.FindAsync(id);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.EmailAccounts.Where(a => a.Name == name && a.IsActive);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public void Add(EmailAccount account) => _context.EmailAccounts.Add(account);

    public void Update(EmailAccount account) => _context.Entry(account).State = EntityState.Modified;

    public async Task ClearDefaultAsync()
        => await _context.EmailAccounts.Where(a => a.IsDefault).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));

    public async Task ClearDefaultExceptAsync(int id)
        => await _context.EmailAccounts.Where(a => a.IsDefault && a.Id != id).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
}
