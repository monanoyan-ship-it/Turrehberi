using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Messages;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Messages;

public class MessageTemplateFactory : IMessageTemplateFactory
{
    private readonly IMessageTemplateEntityService _templateService;
    private readonly IUnitOfWork _unitOfWork;

    public MessageTemplateFactory(IMessageTemplateEntityService templateService, IUnitOfWork unitOfWork)
    {
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetAllAsync(int companyId)
    {
        var templates = await _templateService.GetByCompanyId(companyId)
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Content,
                t.DisplayOrder,
                t.UsageCount,
                t.CreatedAt
            })
            .ToListAsync();

        return new { templates };
    }

    public async Task<(bool success, string message, object? data)> CreateAsync(
        int companyId, string title, string content, int displayOrder)
    {
        var template = new MessageTemplate
        {
            CompanyId = companyId,
            Title = title,
            Content = content,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _templateService.Add(template);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Sablon olusturuldu", new { template.Id });
    }

    public async Task<(bool success, string message)> UpdateAsync(
        int companyId, int templateId, string title, string content, int displayOrder)
    {
        var template = await _templateService.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
            return (false, "Sablon bulunamadi");

        template.Title = title;
        template.Content = content;
        template.DisplayOrder = displayOrder;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Sablon guncellendi");
    }

    public async Task<(bool success, string message)> DeleteAsync(int companyId, int templateId)
    {
        var template = await _templateService.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
            return (false, "Sablon bulunamadi");

        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Sablon silindi");
    }

    public async Task<(bool success, string message)> IncrementUsageAsync(int templateId)
    {
        var template = await _templateService.GetByIdAsync(templateId);
        if (template == null)
            return (false, "Sablon bulunamadi");

        template.UsageCount++;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Kullanim sayisi arttirildi");
    }
}
