using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Faqs;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Faqs;

public class FaqFactory : IFaqFactory
{
    private readonly IFaqEntityService _service;
    private readonly IUnitOfWork _unitOfWork;

    public FaqFactory(IFaqEntityService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        return await _service.GetActiveFaqs()
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new
            {
                f.Id,
                f.Question,
                f.Answer,
                f.Category,
                f.DisplayOrder,
                f.IsActive,
                f.CreatedAt
            })
            .ToListAsync<object>();
    }

    public async Task<IEnumerable<object>> GetPublicAsync()
    {
        return await _service.GetActiveFaqs()
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new
            {
                f.Id,
                f.Question,
                f.Answer,
                f.Category
            })
            .ToListAsync<object>();
    }

    public async Task<Faq?> GetByIdAsync(int id)
        => await _service.GetByIdAsync(id);

    public async Task<Faq> CreateAsync(Faq faq)
    {
        faq.CreatedAt = DateTime.UtcNow;
        _service.Add(faq);
        await _unitOfWork.SaveChangesAsync();
        return faq;
    }

    public async Task<(bool success, string message, int statusCode)> UpdateAsync(int id, Faq faq)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return (false, "Error.FaqNotFound", 404);

        existing.Question = faq.Question;
        existing.Answer = faq.Answer;
        existing.Category = faq.Category;
        existing.DisplayOrder = faq.DisplayOrder;
        existing.UpdatedAt = DateTime.UtcNow;

        _service.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.Updated", 200);
    }

    public async Task<(bool success, string message, int statusCode)> DeleteAsync(int id)
    {
        var faq = await _service.GetByIdAsync(id);
        if (faq == null)
            return (false, "Error.FaqNotFound", 404);

        faq.IsActive = false;
        faq.UpdatedAt = DateTime.UtcNow;
        _service.Update(faq);
        await _unitOfWork.SaveChangesAsync();
        return (true, "Success.Deleted", 200);
    }
}
