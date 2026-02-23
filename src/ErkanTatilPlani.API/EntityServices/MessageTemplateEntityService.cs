using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;

namespace ErkanTatilPlani.API.EntityServices;

public class MessageTemplateEntityService : IMessageTemplateEntityService
{
    private readonly AppDbContext _context;

    public MessageTemplateEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<MessageTemplate> GetByCompanyId(int companyId)
        => _context.MessageTemplates.Where(t => t.CompanyId == companyId && t.IsActive);

    public async Task<MessageTemplate?> GetByIdAsync(int id)
        => await _context.MessageTemplates.FindAsync(id);

    public void Add(MessageTemplate template) => _context.MessageTemplates.Add(template);
    public void Remove(MessageTemplate template) => _context.MessageTemplates.Remove(template);
}
