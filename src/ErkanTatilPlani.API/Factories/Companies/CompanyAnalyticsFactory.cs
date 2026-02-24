using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Companies;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Companies;

public class CompanyAnalyticsFactory : ICompanyAnalyticsFactory
{
    private readonly IReservationEntityService _reservationService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;

    public CompanyAnalyticsFactory(
        IReservationEntityService reservationService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService)
    {
        _reservationService = reservationService;
        _tourService = tourService;
        _visitorService = visitorService;
    }

    public async Task<(bool success, object result, int statusCode)> GetRevenueChartAsync(int visitorId, int months = 12)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var tourIds = await GetCompanyTourIds(check.companyId!.Value);
        var startDate = DateTime.UtcNow.AddMonths(-months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var reservations = await _reservationService.GetByTourIds(tourIds)
            .Where(r => r.IsActive &&
                        r.Status != ReservationStatuses.Ids.Cancelled &&
                        r.CreatedAt >= startDate)
            .ToListAsync();

        var chartData = Enumerable.Range(0, months).Select(i =>
        {
            var m = startDate.AddMonths(i);
            var monthReservations = reservations.Where(r =>
                r.CreatedAt.Year == m.Year && r.CreatedAt.Month == m.Month);
            return new
            {
                Label = m.ToString("MMM yyyy"),
                Year = m.Year,
                Month = m.Month,
                Revenue = monthReservations.Sum(r => r.TotalPrice),
                Count = monthReservations.Count()
            };
        }).ToList();

        return (true, chartData, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetOccupancyChartAsync(int visitorId, int months = 12)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var tourIds = await GetCompanyTourIds(check.companyId!.Value);
        var startDate = DateTime.UtcNow.AddMonths(-months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startDateOnly = DateOnly.FromDateTime(startDate);

        // Doluluk icin rezervasyon verilerini kullan
        var reservations = await _reservationService.GetByTourIds(tourIds)
            .Where(r => r.IsActive &&
                        r.Date >= startDateOnly &&
                        r.Status != ReservationStatuses.Ids.Cancelled)
            .ToListAsync();

        // Turlarin kapasitelerini al
        var tours = await _tourService.GetByCompanyId(check.companyId!.Value)
            .Select(t => new { t.Id, t.MaxCapacity })
            .ToListAsync();

        var chartData = Enumerable.Range(0, months).Select(i =>
        {
            var m = startDate.AddMonths(i);
            var monthRes = reservations.Where(r =>
                r.Date.Year == m.Year && r.Date.Month == m.Month);
            var totalBooked = monthRes.Sum(r => r.NumberOfPeople);
            // Toplam kapasite: her turun kapasitesi * o aydaki gun sayisi (basit tahmin)
            var totalCapacity = tours.Sum(t => t.MaxCapacity) * DateTime.DaysInMonth(m.Year, m.Month);
            return new
            {
                Label = m.ToString("MMM yyyy"),
                Year = m.Year,
                Month = m.Month,
                OccupancyRate = totalCapacity > 0 ? Math.Round((decimal)totalBooked / totalCapacity * 100, 1) : 0,
                TotalCapacity = totalCapacity,
                TotalBooked = totalBooked
            };
        }).ToList();

        return (true, chartData, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetCancellationChartAsync(int visitorId, int months = 12)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var tourIds = await GetCompanyTourIds(check.companyId!.Value);
        var startDate = DateTime.UtcNow.AddMonths(-months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var reservations = await _reservationService.GetByTourIds(tourIds)
            .Where(r => r.IsActive && r.CreatedAt >= startDate)
            .ToListAsync();

        var chartData = Enumerable.Range(0, months).Select(i =>
        {
            var m = startDate.AddMonths(i);
            var monthRes = reservations.Where(r =>
                r.CreatedAt.Year == m.Year && r.CreatedAt.Month == m.Month);
            var total = monthRes.Count();
            var cancelled = monthRes.Count(r => r.Status == ReservationStatuses.Ids.Cancelled);
            return new
            {
                Label = m.ToString("MMM yyyy"),
                Year = m.Year,
                Month = m.Month,
                CancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 1) : 0,
                TotalReservations = total,
                Cancelled = cancelled
            };
        }).ToList();

        return (true, chartData, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetTourComparisonAsync(int visitorId)
    {
        var check = await CheckCompanyOwnership(visitorId);
        if (check.errorMessage != null) return (false, new { message = check.errorMessage }, check.statusCode);

        var tours = await _tourService.GetByCompanyId(check.companyId!.Value)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        var tourIds = tours.Select(t => t.Id).ToList();

        var reservations = await _reservationService.GetByTourIds(tourIds)
            .Where(r => r.IsActive && r.Status != ReservationStatuses.Ids.Cancelled)
            .ToListAsync();

        var comparison = tours.Select(t => new
        {
            t.Name,
            Reservations = reservations.Count(r => r.TourId == t.Id),
            Revenue = reservations.Where(r => r.TourId == t.Id).Sum(r => r.TotalPrice),
            Visitors = reservations.Where(r => r.TourId == t.Id).Sum(r => r.NumberOfPeople)
        }).OrderByDescending(x => x.Revenue).ToList();

        return (true, comparison, 200);
    }

    private async Task<List<int>> GetCompanyTourIds(int companyId)
    {
        return await _tourService.GetByCompanyId(companyId)
            .Select(t => t.Id)
            .ToListAsync();
    }

    private async Task<(string? errorMessage, int statusCode, int? companyId)> CheckCompanyOwnership(int visitorId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return ("Error.UserNotFound", 401, null);
        if (visitor.Company == null) return ("Error.NotCompanyOwner", 403, null);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return ("Error.CompanyStatusInvalid", 403, null);

        return (null, 200, visitor.Company.Id);
    }
}
