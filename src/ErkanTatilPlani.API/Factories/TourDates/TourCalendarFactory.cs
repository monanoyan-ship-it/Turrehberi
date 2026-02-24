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
    private readonly ITourScheduleFactory _scheduleFactory;

    public TourCalendarFactory(
        ITourDateEntityService tourDateService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        ITourScheduleFactory scheduleFactory)
    {
        _tourDateService = tourDateService;
        _tourService = tourService;
        _visitorService = visitorService;
        _scheduleFactory = scheduleFactory;
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

        var companyTours = await _tourService.GetByCompanyId(companyId)
            .Where(t => t.IsActive)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        var tourIds = tourId.HasValue
            ? companyTours.Where(t => t.Id == tourId.Value).Select(t => t.Id).ToList()
            : companyTours.Select(t => t.Id).ToList();

        // Schedule-aware: her tur icin sanal tarihler uret
        var allVirtualDates = new List<(VirtualTourDateDto dto, string tourName)>();
        foreach (var tid in tourIds)
        {
            var tourName = companyTours.First(t => t.Id == tid).Name;
            var virtualDates = await _scheduleFactory.GenerateVirtualDatesAsync(tid, year, month);
            foreach (var vd in virtualDates)
                allVirtualDates.Add((vd, tourName));
        }

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dayData = Enumerable.Range(1, daysInMonth).Select(day =>
        {
            var dayDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            var dayDates = allVirtualDates
                .Where(x => x.dto.StartDate.Date == dayDate.Date)
                .OrderBy(x => x.dto.StartDate)
                .Select(x => new
                {
                    x.dto.Id,
                    x.dto.TourId,
                    TourName = x.tourName,
                    x.dto.StartDate,
                    x.dto.EndDate,
                    x.dto.Price,
                    x.dto.MaxCapacity,
                    x.dto.BookedCount,
                    x.dto.IsAvailable,
                    x.dto.IsCancelled,
                    x.dto.Token,
                    x.dto.IsVirtual,
                    Remaining = x.dto.MaxCapacity.HasValue ? x.dto.MaxCapacity.Value - x.dto.BookedCount : (int?)null,
                    OccupancyPercent = x.dto.MaxCapacity.HasValue && x.dto.MaxCapacity.Value > 0
                        ? Math.Round((decimal)x.dto.BookedCount / x.dto.MaxCapacity.Value * 100, 1) : 0
                })
                .ToList();
            return new
            {
                Day = day,
                Date = dayDate,
                IsPast = dayDate < DateTime.UtcNow.Date,
                Tours = (object)dayDates
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
