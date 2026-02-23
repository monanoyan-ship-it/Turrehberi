namespace ErkanTatilPlani.Core.Factories.Messages;

public interface IMessageTemplateFactory
{
    Task<object> GetAllAsync(int companyId);
    Task<(bool success, string message, object? data)> CreateAsync(int companyId, string title, string content, int displayOrder);
    Task<(bool success, string message)> UpdateAsync(int companyId, int templateId, string title, string content, int displayOrder);
    Task<(bool success, string message)> DeleteAsync(int companyId, int templateId);
    Task<(bool success, string message)> IncrementUsageAsync(int templateId);
}
