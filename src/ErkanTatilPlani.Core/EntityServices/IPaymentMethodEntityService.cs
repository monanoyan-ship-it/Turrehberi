using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IPaymentMethodEntityService
{
    IQueryable<PaymentMethodSetting> GetActiveMethods();
    IQueryable<PaymentMethodSetting> GetAllMethods();
    Task<PaymentMethodSetting?> GetByIdAsync(int id);
    Task<PaymentMethodSetting?> GetBySystemNameAsync(string systemName);
    Task<PaymentMethodSetting?> GetDefaultMethodAsync();
    Task<bool> AnyEnabledMethodAsync();
    Task ClearDefaultFlagsAsync();
    void Add(PaymentMethodSetting method);
    void Update(PaymentMethodSetting method);
}
