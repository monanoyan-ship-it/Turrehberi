using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Visitors;

public interface IVisitorFactory
{
    Task<IEnumerable<Visitor>> GetAllAsync();
    Task<Visitor?> GetByIdAsync(int id);
    Task<Visitor> CreateAsync(Visitor visitor);
    Task UpdateAsync(int id, Visitor visitor);
}
