using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Visitors;
using ErkanTatilPlani.Core.Infrastructure;

namespace ErkanTatilPlani.API.Factories.Visitors;

public class VisitorFactory : IVisitorFactory
{
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public VisitorFactory(IVisitorEntityService visitorService, IUnitOfWork unitOfWork)
    {
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Visitor>> GetAllAsync()
        => await _visitorService.GetActiveVisitorsAsync();

    public async Task<Visitor?> GetByIdAsync(int id)
        => await _visitorService.GetByIdAsync(id);

    public async Task<Visitor> CreateAsync(Visitor visitor)
    {
        _visitorService.Add(visitor);
        await _unitOfWork.SaveChangesAsync();
        return visitor;
    }

    public async Task UpdateAsync(int id, Visitor visitor)
    {
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);
        await _unitOfWork.SaveChangesAsync();
    }
}
