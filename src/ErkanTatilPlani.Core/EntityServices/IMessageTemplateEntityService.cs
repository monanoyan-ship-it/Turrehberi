using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IMessageTemplateEntityService
{
    IQueryable<MessageTemplate> GetByCompanyId(int companyId);
    Task<MessageTemplate?> GetByIdAsync(int id);
    void Add(MessageTemplate template);
    void Remove(MessageTemplate template);
}
