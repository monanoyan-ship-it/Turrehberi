using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.TourDates;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ErkanTatilPlani.API.Factories.TourDates;

public class TourScheduleFactory : ITourScheduleFactory
{
    private readonly ITourScheduleEntityService _scheduleService;
    private readonly IReservationEntityService _reservationService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public TourScheduleFactory(
        ITourScheduleEntityService scheduleService,
        IReservationEntityService reservationService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _scheduleService = scheduleService;
        _reservationService = reservationService;
        _tourService = tourService;
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object result, int statusCode)> GetSchedulesAsync(int visitorId, int tourId)
    {
        var check = await CheckTourOwnership(visitorId, tourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        var schedules = await _scheduleService.GetByTourId(tourId)
            .Where(s => s.IsActive)
            .OrderBy(s => s.ValidFrom)
            .ThenBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                s.TourId,
                s.DaysOfWeekJson,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                s.DurationValue,
                DurationUnit = DurationUnits.GetById(s.DurationUnitId) != null
                    ? DurationUnits.GetById(s.DurationUnitId)!.SystemName : "Day",
                s.Price,
                s.MaxCapacity,
                s.ValidFrom,
                s.ValidTo,
                s.CreatedAt
            })
            .ToListAsync();

        return (true, schedules, 200);
    }

    public async Task<(bool success, object result, int statusCode)> CreateScheduleAsync(int visitorId, int tourId, CreateScheduleRequest request)
    {
        var check = await CheckTourOwnership(visitorId, tourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        if (request.DaysOfWeek == null || !request.DaysOfWeek.Any())
            return (false, new { message = "Validation.DaysOfWeekRequired" }, 400);

        if (request.ValidFrom >= request.ValidTo)
            return (false, new { message = "Validation.ValidDateRangeRequired" }, 400);

        if (!TimeSpan.TryParse(request.StartTime, out var startTime))
            return (false, new { message = "Validation.InvalidTimeFormat" }, 400);

        if (request.Price <= 0)
            return (false, new { message = "Schedule.PriceRequired" }, 400);

        var durationUnit = DurationUnits.GetBySystemName(request.DurationUnit ?? "Day");
        if (durationUnit == null)
            return (false, new { message = "Validation.InvalidDurationUnit" }, 400);

        // DurationUnitId kilitleme: mevcut aktif schedule varsa ayni birim zorunlu
        var existingSchedules = await _scheduleService.GetByTourId(tourId)
            .Where(s => s.IsActive)
            .ToListAsync();

        if (existingSchedules.Any())
        {
            var lockedUnitId = existingSchedules.First().DurationUnitId;
            if (durationUnit.Id != lockedUnitId)
                return (false, new { message = "Tour.DurationUnitLocked" }, 400);
        }

        var schedule = new TourSchedule
        {
            TourId = tourId,
            DaysOfWeekJson = JsonSerializer.Serialize(request.DaysOfWeek),
            StartTime = startTime,
            DurationValue = request.DurationValue > 0 ? request.DurationValue : 1,
            DurationUnitId = durationUnit.Id,
            Price = request.Price,
            MaxCapacity = request.MaxCapacity,
            ValidFrom = DateTime.SpecifyKind(request.ValidFrom.Date, DateTimeKind.Utc),
            ValidTo = DateTime.SpecifyKind(request.ValidTo.Date, DateTimeKind.Utc)
        };

        _scheduleService.Add(schedule);
        await _unitOfWork.SaveChangesAsync();

        // Tour.Price cache'ini guncelle
        await SyncTourPriceAsync(tourId);

        return (true, new
        {
            message = "TourSchedule.Created",
            schedule = new
            {
                schedule.Id,
                schedule.TourId,
                schedule.DaysOfWeekJson,
                StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                schedule.DurationValue,
                DurationUnit = durationUnit.SystemName,
                schedule.Price,
                schedule.MaxCapacity,
                schedule.ValidFrom,
                schedule.ValidTo
            }
        }, 201);
    }

    public async Task<(bool success, object result, int statusCode)> UpdateScheduleAsync(int visitorId, int id, UpdateScheduleRequest request)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        if (schedule == null || !schedule.IsActive)
            return (false, new { message = "Error.ScheduleNotFound" }, 404);

        var check = await CheckTourOwnership(visitorId, schedule.TourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        if (request.DaysOfWeek != null)
        {
            if (!request.DaysOfWeek.Any())
                return (false, new { message = "Validation.DaysOfWeekRequired" }, 400);
            schedule.DaysOfWeekJson = JsonSerializer.Serialize(request.DaysOfWeek);
        }

        if (request.StartTime != null)
        {
            if (!TimeSpan.TryParse(request.StartTime, out var startTime))
                return (false, new { message = "Validation.InvalidTimeFormat" }, 400);
            schedule.StartTime = startTime;
        }

        if (request.DurationValue.HasValue)
            schedule.DurationValue = request.DurationValue.Value > 0 ? request.DurationValue.Value : 1;

        if (request.DurationUnit != null)
        {
            var unit = DurationUnits.GetBySystemName(request.DurationUnit);
            if (unit == null)
                return (false, new { message = "Validation.InvalidDurationUnit" }, 400);

            // DurationUnitId kilitleme: diger aktif schedule'larla uyumlu olmali
            var otherSchedules = await _scheduleService.GetByTourId(schedule.TourId)
                .Where(s => s.IsActive && s.Id != schedule.Id)
                .ToListAsync();

            if (otherSchedules.Any())
            {
                var lockedUnitId = otherSchedules.First().DurationUnitId;
                if (unit.Id != lockedUnitId)
                    return (false, new { message = "Tour.DurationUnitLocked" }, 400);
            }

            schedule.DurationUnitId = unit.Id;
        }

        if (request.Price.HasValue)
        {
            if (request.Price.Value <= 0)
                return (false, new { message = "Schedule.PriceRequired" }, 400);
            schedule.Price = request.Price.Value;
        }

        if (request.MaxCapacity.HasValue)
            schedule.MaxCapacity = request.MaxCapacity;

        if (request.ValidFrom.HasValue)
            schedule.ValidFrom = DateTime.SpecifyKind(request.ValidFrom.Value.Date, DateTimeKind.Utc);

        if (request.ValidTo.HasValue)
            schedule.ValidTo = DateTime.SpecifyKind(request.ValidTo.Value.Date, DateTimeKind.Utc);

        if (schedule.ValidFrom >= schedule.ValidTo)
            return (false, new { message = "Validation.ValidDateRangeRequired" }, 400);

        schedule.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Tour.Price cache'ini guncelle
        await SyncTourPriceAsync(schedule.TourId);

        return (true, new { message = "TourSchedule.Updated" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeleteScheduleAsync(int visitorId, int id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        if (schedule == null || !schedule.IsActive)
            return (false, new { message = "Error.ScheduleNotFound" }, 404);

        var check = await CheckTourOwnership(visitorId, schedule.TourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        schedule.IsActive = false;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Tour.Price cache'ini guncelle
        await SyncTourPriceAsync(schedule.TourId);

        return (true, new { message = "TourSchedule.Deleted" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> CancelDateAsync(int visitorId, int tourId, CancelDateRequest request)
    {
        var check = await CheckTourOwnership(visitorId, tourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        if (string.IsNullOrEmpty(request.DateToken))
            return (false, new { message = "Validation.DateTokenRequired" }, 400);

        // Token parse: "s:42:2026-03-15"
        if (!request.DateToken.StartsWith("s:"))
            return (false, new { message = "Error.InvalidDateToken" }, 400);

        var parts = request.DateToken.Split(':');
        if (parts.Length != 3 || !int.TryParse(parts[1], out var scheduleId) || !DateTime.TryParse(parts[2], out _))
            return (false, new { message = "Error.InvalidDateToken" }, 400);

        var schedule = await _scheduleService.GetByIdAsync(scheduleId);
        if (schedule == null || !schedule.IsActive)
            return (false, new { message = "Error.ScheduleNotFound" }, 404);

        if (schedule.TourId != tourId)
            return (false, new { message = "Error.SessionNotBelongToTour" }, 400);

        // TODO: Tarih iptal mekanizmasi icin ayri bir CancelledDate tablosu veya
        // schedule'a cancelled_dates JSON alani eklenebilir. Simdilik schedule'i deaktive et.
        schedule.IsActive = false;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "TourSchedule.DateCancelled" }, 200);
    }

    public async Task<List<VirtualTourDateDto>> GenerateVirtualDatesAsync(int tourId, int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        // 1. Aktif schedule'lari al (ay ile kesisen)
        var schedules = await _scheduleService.GetActiveByTourId(tourId)
            .Where(s => s.ValidFrom < monthEnd && s.ValidTo >= monthStart)
            .ToListAsync();

        // 2. Bu ay icin mevcut rezervasyonlari al (doluluk hesabi icin)
        var monthStartDate = new DateOnly(year, month, 1);
        var monthEndDate = monthStartDate.AddMonths(1);

        var reservations = await _reservationService.GetActiveReservations()
            .Where(r => r.TourId == tourId &&
                        r.Date >= monthStartDate && r.Date < monthEndDate &&
                        r.Status != ReservationStatuses.Ids.Cancelled)
            .Select(r => new { r.ScheduleId, r.Date, r.NumberOfPeople })
            .ToListAsync();

        // Tour bilgisi (fallback fiyat/kapasite icin)
        var tour = await _tourService.GetByIdAsync(tourId);

        var result = new List<VirtualTourDateDto>();

        // 3. Her gun icin schedule'lardan sanal tarih uret
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            var dateOnly = new DateOnly(year, month, day);
            var dayOfWeek = (int)date.DayOfWeek;

            foreach (var schedule in schedules)
            {
                // Gecerlilik kontrolu
                if (date < schedule.ValidFrom.Date || date > schedule.ValidTo.Date)
                    continue;

                // Gun eslesmesi
                var daysOfWeek = ParseDaysOfWeek(schedule.DaysOfWeekJson);
                if (!daysOfWeek.Contains(dayOfWeek))
                    continue;

                // Bu schedule + tarih icin doluluk hesapla
                var bookedCount = reservations
                    .Where(r => r.ScheduleId == schedule.Id && r.Date == dateOnly)
                    .Sum(r => r.NumberOfPeople);

                var maxCapacity = schedule.MaxCapacity ?? tour?.MaxCapacity;
                var isAvailable = date.Add(schedule.StartTime) > DateTime.UtcNow &&
                    (!maxCapacity.HasValue || bookedCount < maxCapacity.Value);

                result.Add(new VirtualTourDateDto
                {
                    TourId = tourId,
                    Date = dateOnly,
                    StartTime = schedule.StartTime,
                    DurationValue = schedule.DurationValue,
                    DurationUnitId = schedule.DurationUnitId,
                    Price = schedule.Price,
                    MaxCapacity = maxCapacity,
                    BookedCount = bookedCount,
                    IsAvailable = isAvailable,
                    IsCancelled = false,
                    Token = $"s:{schedule.Id}:{dateOnly:yyyy-MM-dd}",
                    ScheduleId = schedule.Id,
                    IsVirtual = true
                });
            }
        }

        return result.OrderBy(r => r.Date).ThenBy(r => r.StartTime).ToList();
    }

    public async Task<IEnumerable<object>> GetTourDatesAsync(int tourId, DateOnly? from = null, DateOnly? to = null, bool onlyWithReservations = false)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Default: bugunden 3 ay ileri
        var startDate = from ?? today;
        var endDate = to ?? DateOnly.FromDateTime(now.AddMonths(3));

        // Ay bazinda GenerateVirtualDatesAsync cagir
        var allDates = new List<VirtualTourDateDto>();
        var current = new DateOnly(startDate.Year, startDate.Month, 1);
        var lastMonth = new DateOnly(endDate.Year, endDate.Month, 1);

        while (current <= lastMonth)
        {
            var virtualDates = await GenerateVirtualDatesAsync(tourId, current.Year, current.Month);
            allDates.AddRange(virtualDates);
            current = current.AddMonths(1);
        }

        // Tarih araligina filtrele
        var filtered = allDates.Where(d => d.Date >= startDate && d.Date <= endDate);

        // from belirtilmemisse (public kullanim) sadece gelecek + musait
        if (!from.HasValue)
            filtered = filtered.Where(d => (d.Date > today || (d.Date == today && d.StartTime > now.TimeOfDay)) && d.IsAvailable);

        // Sadece rezervasyonu olanlar
        if (onlyWithReservations)
            filtered = filtered.Where(d => d.BookedCount > 0);

        return filtered
            .OrderBy(d => d.Date).ThenBy(d => d.StartTime)
            .Select(d => new
            {
                d.TourId,
                d.Date,
                StartTime = d.StartTime.ToString(@"hh\:mm"),
                d.DurationValue,
                DurationUnit = DurationUnits.GetById(d.DurationUnitId)?.SystemName ?? "Day",
                d.Price, d.MaxCapacity, d.BookedCount, d.IsAvailable,
                d.IsCancelled, d.Token, d.ScheduleId, d.IsVirtual
            });
    }

    public async Task<IEnumerable<object>> GetCheapestDatesAsync(int tourId, string month)
    {
        if (!DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var monthStart))
            return Array.Empty<object>();

        var virtualDates = await GenerateVirtualDatesAsync(tourId, monthStart.Year, monthStart.Month);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return virtualDates
            .Where(d => d.Date >= today && d.IsAvailable)
            .OrderBy(d => d.Price)
            .ThenBy(d => d.Date)
            .Select(d => new
            {
                d.TourId,
                d.Date,
                StartTime = d.StartTime.ToString(@"hh\:mm"),
                d.DurationValue,
                DurationUnit = DurationUnits.GetById(d.DurationUnitId)?.SystemName ?? "Day",
                d.Price, d.MaxCapacity, d.BookedCount, d.IsAvailable,
                d.Token, d.IsVirtual
            });
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
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var companyTours = await _tourService.GetByCompanyId(companyId)
            .Where(t => t.IsActive)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        var tourIds = tourId.HasValue
            ? companyTours.Where(t => t.Id == tourId.Value).Select(t => t.Id).ToList()
            : companyTours.Select(t => t.Id).ToList();

        var allVirtualDates = new List<(VirtualTourDateDto dto, string tourName)>();
        foreach (var tid in tourIds)
        {
            var tourName = companyTours.First(t => t.Id == tid).Name;
            var virtualDates = await GenerateVirtualDatesAsync(tid, year, month);
            foreach (var vd in virtualDates)
                allVirtualDates.Add((vd, tourName));
        }

        var dayData = Enumerable.Range(1, daysInMonth).Select(day =>
        {
            var dayDate = new DateOnly(year, month, day);
            var dayDates = allVirtualDates
                .Where(x => x.dto.Date == dayDate)
                .OrderBy(x => x.dto.StartTime)
                .Select(x => new
                {
                    x.dto.TourId,
                    TourName = x.tourName,
                    x.dto.Date,
                    StartTime = x.dto.StartTime.ToString(@"hh\:mm"),
                    x.dto.DurationValue,
                    DurationUnit = DurationUnits.GetById(x.dto.DurationUnitId)?.SystemName ?? "Day",
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
                IsPast = dayDate < DateOnly.FromDateTime(DateTime.UtcNow),
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

    public async Task<(bool success, object result, int statusCode)> GetLockedDurationUnitAsync(int visitorId, int tourId)
    {
        var check = await CheckTourOwnership(visitorId, tourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        var existingSchedule = await _scheduleService.GetByTourId(tourId)
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        if (existingSchedule == null)
            return (true, new { locked = false, durationUnitId = (int?)null, durationUnit = (string?)null }, 200);

        var unit = DurationUnits.GetById(existingSchedule.DurationUnitId);
        return (true, new
        {
            locked = true,
            durationUnitId = existingSchedule.DurationUnitId,
            durationUnit = unit?.SystemName
        }, 200);
    }

    private async Task SyncTourPriceAsync(int tourId)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return;

        var minPrice = await _scheduleService.GetByTourId(tourId)
            .Where(s => s.IsActive)
            .Select(s => (decimal?)s.Price)
            .MinAsync();

        tour.Price = minPrice ?? 0;
        tour.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    private static List<int> ParseDaysOfWeek(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }

    private async Task<(string? errorMessage, string? errorCode, int? statusCode)> CheckTourOwnership(int visitorId, int tourId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return ("Error.UserNotFound", null, 401);
        if (visitor.Company == null) return ("Error.NotCompanyOwner", "NOT_COMPANY_OWNER", 403);
        if (visitor.Company.StatusId != CompanyStatuses.Ids.Approved)
            return ("Error.CompanyStatusInvalid", "COMPANY_NOT_APPROVED", 403);

        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null) return ("Error.TourNotFound", null, 404);
        if (tour.CompanyId != visitor.Company.Id) return ("Error.TourNotOwnedByCompany", "NOT_TOUR_OWNER", 403);

        return (null, null, null);
    }
}
