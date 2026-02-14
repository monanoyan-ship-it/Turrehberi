namespace ErkanTatilPlani.Core.Factories.Companies;

public interface ICompanyDashboardFactory
{
    Task<(bool found, object? result)> GetCompanyDashboardAsync(int id);
    Task<(bool success, object? result, int statusCode)> UpdateCompanySettingsAsync(int id, int depositPercentage);
    Task<(bool success, object? result, int statusCode)> UploadContractAsync(int id, Stream fileStream, string fileName, string contentType, long fileLength);
    Task<(bool success, object? result, int statusCode)> DeleteContractAsync(int id);
}
