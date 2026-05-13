using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class PaymentMethodEntityService : IPaymentMethodEntityService
{
    private readonly AppDbContext _context;

    public PaymentMethodEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<PaymentMethodSetting> GetActiveMethods()
        => _context.PaymentMethodSettings.Where(x => x.IsActive);

    public IQueryable<PaymentMethodSetting> GetAllMethods()
        => _context.PaymentMethodSettings;

    public async Task<PaymentMethodSetting?> GetByIdAsync(int id)
        => await _context.PaymentMethodSettings.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

    public async Task<PaymentMethodSetting?> GetBySystemNameAsync(string systemName)
        => await _context.PaymentMethodSettings.FirstOrDefaultAsync(x =>
            x.IsActive && x.SystemName == systemName);

    public async Task<PaymentMethodSetting?> GetDefaultMethodAsync()
        => await _context.PaymentMethodSettings.FirstOrDefaultAsync(x => x.IsActive && x.IsEnabled && x.IsDefault);

    public async Task<bool> AnyEnabledMethodAsync()
        => await _context.PaymentMethodSettings.AnyAsync(x => x.IsActive && x.IsEnabled);

    public async Task ClearDefaultFlagsAsync()
    {
        var defaults = await _context.PaymentMethodSettings
            .Where(x => x.IsActive && x.IsDefault)
            .ToListAsync();

        foreach (var method in defaults)
        {
            method.IsDefault = false;
            method.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Add(PaymentMethodSetting method) => _context.PaymentMethodSettings.Add(method);

    public void Update(PaymentMethodSetting method) => _context.Entry(method).State = EntityState.Modified;
}
