using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class FaqEntityService : IFaqEntityService
{
    private readonly AppDbContext _context;

    public FaqEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Faq> GetActiveFaqs()
        => _context.Faqs.Where(f => f.IsActive);

    public async Task<Faq?> GetByIdAsync(int id)
        => await _context.Faqs.FindAsync(id);

    public void Add(Faq faq) => _context.Faqs.Add(faq);

    public void Update(Faq faq)
        => _context.Entry(faq).State = EntityState.Modified;
}
