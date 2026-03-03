using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Faqs;

public interface IFaqFactory
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<IEnumerable<object>> GetPublicAsync();
    Task<Faq?> GetByIdAsync(int id);
    Task<Faq> CreateAsync(Faq faq);
    Task<(bool success, string message, int statusCode)> UpdateAsync(int id, Faq faq);
    Task<(bool success, string message, int statusCode)> DeleteAsync(int id);
}
