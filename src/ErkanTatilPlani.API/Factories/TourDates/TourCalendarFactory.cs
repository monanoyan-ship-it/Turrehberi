using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.TourDates;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.TourDates;

public class TourCalendarFactory : ITourCalendarFactory
{
    private readonly ITourDateEntityService _tourDateService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;

    public TourCalendarFactory(
        ITourDateEntityService tourDateService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService)
    {
        _tourDateService = tourDateService;
        _tourService = tourService;
        _visitorService = visitorService;
    }

    public async Task<(bool success, object result, int statusCode)> GetCalendarDataAsync(int visitorId, int year, int month, int? tourId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return (false, new { message = "Error.UserNotFound" }, 401);
        if (visitor.Company == null) return (false, new { message = "Error.NotCompanyOwner" }, 403);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, new { message = "Error.CompanyStatusInvalid" }, 403);

        var companyId = visitor.Company.Id;
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var companyTours = await _tourService.GetByCompanyId(companyId)
            .Where(t => t.IsActive)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        var tourIds = tourId.HasValue
            ? companyTours.Where(t => t.Id == tourId.Value).Select(t => t.Id).ToList()
            : companyTours.Select(t => t.Id).ToList();

        var dates = await _tourDateService.GetByTourIds(tourIds)
            .Where(td => td.StartDate >= monthStart && td.StartDate < monthEnd)
            .Include(td => td.Tour)
            .OrderBy(td => td.StartDate)
            .Select(td => new
            {
                td.Id, td.TourId,
                TourName = td.Tour.Name,
                td.StartDate, td.EndDate,
                td.Price, td.MaxCapacity, td.BookedCount, td.IsAvailable,
                Remaining = td.MaxCapacity.HasValue ? td.MaxCapacity.Value - td.BookedCount : (int?)null,
                OccupancyPercent = td.MaxCapacity.HasValue && td.MaxCapacity.Value > 0
                    ? Math.Round((decimal)td.BookedCount / td.MaxCapacity.Value * 100, 1) : 0
            })
            .ToListAsync();

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dayData = Enumerable.Range(1, daysInMonth).Select(day =>
        {
            var dayDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            var dayDates = dates.Where(d => d.StartDate.Date == dayDate.Date).ToList();
            return new
            {
                Day = day,
                Date = dayDate,
                IsPast = dayDate < DateTime.UtcNow.Date,
                Tours = dayDates
            };
        }).ToList();

        return (true, new
        {
            year, month, daysInMonth,
            firstDayOfWeek = (int)monthStart.DayOfWeek,
            tours = companyTours,
            days = dayData
        }, 200);
    }
}
