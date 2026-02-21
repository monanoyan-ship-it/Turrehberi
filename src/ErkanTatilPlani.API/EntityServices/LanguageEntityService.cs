using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class LanguageEntityService : ILanguageEntityService
{
    private readonly AppDbContext _context;

    public LanguageEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Language> GetActiveLanguages()
        => _context.Languages.Where(l => l.IsActive);

    public async Task<IEnumerable<Language>> GetAllActiveAsync()
        => await _context.Languages.Where(l => l.IsActive).ToListAsync();

    public async Task<Language?> GetByIdAsync(int id)
        => await _context.Languages.FindAsync(id);

    public void Add(Language language) => _context.Languages.Add(language);

    public void Update(Language language) => _context.Entry(language).State = EntityState.Modified;

    public async Task ClearDefaultAsync()
        => await _context.Languages.Where(l => l.IsDefault).ExecuteUpdateAsync(s => s.SetProperty(l => l.IsDefault, false));
}
