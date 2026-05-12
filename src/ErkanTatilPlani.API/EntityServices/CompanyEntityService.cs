using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class CompanyEntityService : ICompanyEntityService
{
    private readonly AppDbContext _context;

    public CompanyEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync()
        => await _context.Companies.Where(c => c.IsActive).ToListAsync();

    public async Task<Company?> GetByIdAsync(int id)
        => await _context.Companies.FindAsync(id);

    public async Task<Company?> GetBySlugAsync(string slug)
        => await _context.Companies.FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && c.StatusId == CompanyStatuses.Ids.Approved);

    public async Task<Company?> GetByTaxNumberAsync(string taxNumber)
        => await _context.Companies.FirstOrDefaultAsync(c => c.TaxNumber == taxNumber);

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        => await _context.Companies.AnyAsync(c => c.Slug == slug && (!excludeId.HasValue || c.Id != excludeId.Value));

    public IQueryable<Company> GetActiveApprovedCompanies()
        => _context.Companies.Where(c => c.IsActive && c.StatusId == CompanyStatuses.Ids.Approved);

    public async Task<IEnumerable<Company>> GetPendingCompaniesAsync()
        => await _context.Companies.Where(c => c.IsActive && c.StatusId == CompanyStatuses.Ids.Pending).OrderBy(c => c.ApplicationDate).ToListAsync();

    public void Add(Company company) => _context.Companies.Add(company);

    public void Update(Company company) => _context.Entry(company).State = EntityState.Modified;
}
