namespace ErkanTatilPlani.Core.Factories.Companies;

public interface ICompanyAnalyticsFactory
{
    Task<(bool success, object result, int statusCode)> GetRevenueChartAsync(int visitorId, int months = 12);
    Task<(bool success, object result, int statusCode)> GetOccupancyChartAsync(int visitorId, int months = 12);
    Task<(bool success, object result, int statusCode)> GetCancellationChartAsync(int visitorId, int months = 12);
    Task<(bool success, object result, int statusCode)> GetTourComparisonAsync(int visitorId);
}
