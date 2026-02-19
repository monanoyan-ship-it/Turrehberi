using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class CompanyPageEntityService : ICompanyPageEntityService
{
    private readonly AppDbContext _context;

    public CompanyPageEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<CompanyPage> GetByCompanyId(int companyId)
        => _context.CompanyPages.Where(p => p.CompanyId == companyId && p.IsActive);

    public async Task<CompanyPage?> GetByIdAsync(int id)
        => await _context.CompanyPages.FindAsync(id);

    public async Task<CompanyPage?> GetBySlugAsync(int companyId, string slug)
        => await _context.CompanyPages
            .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Slug == slug && p.IsActive);

    public void Add(CompanyPage page) => _context.CompanyPages.Add(page);

    public void Update(CompanyPage page) => _context.Entry(page).State = EntityState.Modified;
}
