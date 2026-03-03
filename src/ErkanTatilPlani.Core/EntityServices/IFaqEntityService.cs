using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IFaqEntityService
{
    IQueryable<Faq> GetActiveFaqs();
    Task<Faq?> GetByIdAsync(int id);
    void Add(Faq faq);
    void Update(Faq faq);
}
