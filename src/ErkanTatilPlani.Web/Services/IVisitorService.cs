using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Web.Services;

public interface IVisitorService
{
    Task<IEnumerable<Visitor>> GetAllAsync();
    Task<Visitor?> GetByIdAsync(int id);
    Task<Visitor?> CreateAsync(Visitor visitor);
    Task<bool> UpdateAsync(int id, Visitor visitor);
}
