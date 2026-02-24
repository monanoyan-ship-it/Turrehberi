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
    private readonly ITourDateEntityService _tourDateService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public TourScheduleFactory(
        ITourScheduleEntityService scheduleService,
        ITourDateEntityService tourDateService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _scheduleService = scheduleService;
        _tourDateService = tourDateService;
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

        // Validasyon
        if (request.DaysOfWeek == null || !request.DaysOfWeek.Any())
            return (false, new { message = "Validation.DaysOfWeekRequired" }, 400);

        if (request.ValidFrom >= request.ValidTo)
            return (false, new { message = "Validation.ValidDateRangeRequired" }, 400);

        if (!TimeSpan.TryParse(request.StartTime, out var startTime))
            return (false, new { message = "Validation.InvalidTimeFormat" }, 400);

        var durationUnit = DurationUnits.GetBySystemName(request.DurationUnit ?? "Day");
        if (durationUnit == null)
            return (false, new { message = "Validation.InvalidDurationUnit" }, 400);

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
            schedule.DurationUnitId = unit.Id;
        }

        if (request.Price.HasValue)
            schedule.Price = request.Price;

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

        return (true, new { message = "TourSchedule.Deleted" }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> CancelDateAsync(int visitorId, int tourId, CancelDateRequest request)
    {
        var check = await CheckTourOwnership(visitorId, tourId);
        if (check.errorMessage != null)
            return (false, new { message = check.errorMessage }, check.statusCode ?? 400);

        if (string.IsNullOrEmpty(request.DateToken))
            return (false, new { message = "Validation.DateTokenRequired" }, 400);

        // Materialize edip iptal et
        var tourDate = await MaterializeDateAsync(request.DateToken);
        if (tourDate == null)
            return (false, new { message = "Error.DateNotFound" }, 404);

        if (tourDate.TourId != tourId)
            return (false, new { message = "Error.SessionNotBelongToTour" }, 400);

        tourDate.IsCancelled = true;
        tourDate.IsAvailable = false;
        tourDate.UpdatedAt = DateTime.UtcNow;
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

        // 2. Ayda mevcut materialized TourDate'leri al
        var materializedDates = await _tourDateService.GetByTourId(tourId)
            .Where(td => td.StartDate >= monthStart && td.StartDate < monthEnd)
            .Select(td => new MaterializedDateInfo
            {
                Id = td.Id, TourId = td.TourId, StartDate = td.StartDate, EndDate = td.EndDate,
                Price = td.Price, MaxCapacity = td.MaxCapacity, BookedCount = td.BookedCount,
                IsAvailable = td.IsAvailable, ScheduleId = td.ScheduleId, IsCancelled = td.IsCancelled
            })
            .ToListAsync();

        // Tour bilgisi (fallback fiyat/kapasite icin)
        var tour = await _tourService.GetByIdAsync(tourId);

        var result = new List<VirtualTourDateDto>();

        // 3. Her gun icin schedule'lardan sanal tarih uret
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
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

                var startDate = date.Add(schedule.StartTime);
                var endDate = CalculateEndDate(startDate, schedule.DurationValue, schedule.DurationUnitId);

                // Materialized mi?
                var materialized = materializedDates.FirstOrDefault(td =>
                    td.ScheduleId == schedule.Id &&
                    td.StartDate.Date == date.Date);

                if (materialized != null)
                {
                    result.Add(new VirtualTourDateDto
                    {
                        Id = materialized.Id,
                        TourId = tourId,
                        StartDate = materialized.StartDate,
                        EndDate = materialized.EndDate,
                        Price = materialized.Price ?? schedule.Price ?? tour?.Price,
                        MaxCapacity = materialized.MaxCapacity ?? schedule.MaxCapacity ?? tour?.MaxCapacity,
                        BookedCount = materialized.BookedCount,
                        IsAvailable = materialized.IsAvailable && !materialized.IsCancelled,
                        IsCancelled = materialized.IsCancelled,
                        Token = $"d:{materialized.Id}",
                        ScheduleId = schedule.Id,
                        IsVirtual = false
                    });
                }
                else
                {
                    result.Add(new VirtualTourDateDto
                    {
                        Id = 0,
                        TourId = tourId,
                        StartDate = startDate,
                        EndDate = endDate,
                        Price = schedule.Price ?? tour?.Price,
                        MaxCapacity = schedule.MaxCapacity ?? tour?.MaxCapacity,
                        BookedCount = 0,
                        IsAvailable = startDate > DateTime.UtcNow,
                        IsCancelled = false,
                        Token = $"s:{schedule.Id}:{date:yyyy-MM-dd}",
                        ScheduleId = schedule.Id,
                        IsVirtual = true
                    });
                }
            }
        }

        // 4. Legacy TourDate'leri ekle (ScheduleId = null)
        var legacyDates = materializedDates.Where(td => td.ScheduleId == null);
        foreach (var td in legacyDates)
        {
            result.Add(new VirtualTourDateDto
            {
                Id = td.Id,
                TourId = tourId,
                StartDate = td.StartDate,
                EndDate = td.EndDate,
                Price = td.Price ?? tour?.Price,
                MaxCapacity = td.MaxCapacity ?? tour?.MaxCapacity,
                BookedCount = td.BookedCount,
                IsAvailable = td.IsAvailable && !td.IsCancelled,
                IsCancelled = td.IsCancelled,
                Token = $"d:{td.Id}",
                ScheduleId = null,
                IsVirtual = false
            });
        }

        return result.OrderBy(r => r.StartDate).ToList();
    }

    public async Task<TourDate?> MaterializeDateAsync(string dateToken)
    {
        if (string.IsNullOrEmpty(dateToken))
            return null;

        // "d:123" - mevcut TourDate
        if (dateToken.StartsWith("d:"))
        {
            if (int.TryParse(dateToken[2..], out var tourDateId))
                return await _tourDateService.GetByIdAsync(tourDateId);
            return null;
        }

        // "s:42:2026-03-15" - schedule'dan materialize et
        if (dateToken.StartsWith("s:"))
        {
            var parts = dateToken.Split(':');
            if (parts.Length != 3) return null;
            if (!int.TryParse(parts[1], out var scheduleId)) return null;
            if (!DateTime.TryParse(parts[2], out var dateValue)) return null;

            dateValue = DateTime.SpecifyKind(dateValue.Date, DateTimeKind.Utc);

            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null || !schedule.IsActive) return null;

            // Gecerlilik kontrolu
            if (dateValue < schedule.ValidFrom.Date || dateValue > schedule.ValidTo.Date)
                return null;

            // Gun eslesmesi kontrolu
            var daysOfWeek = ParseDaysOfWeek(schedule.DaysOfWeekJson);
            if (!daysOfWeek.Contains((int)dateValue.DayOfWeek))
                return null;

            // Zaten materialized mi? (race condition koruması)
            var existing = await _tourDateService.GetByTourId(schedule.TourId)
                .FirstOrDefaultAsync(td =>
                    td.ScheduleId == scheduleId &&
                    td.StartDate.Date == dateValue.Date);

            if (existing != null)
                return existing;

            // Yeni TourDate olustur
            var startDate = dateValue.Add(schedule.StartTime);
            var endDate = CalculateEndDate(startDate, schedule.DurationValue, schedule.DurationUnitId);

            var tourDate = new TourDate
            {
                TourId = schedule.TourId,
                ScheduleId = scheduleId,
                StartDate = startDate,
                EndDate = endDate,
                Price = schedule.Price,
                MaxCapacity = schedule.MaxCapacity,
                IsAvailable = true,
                BookedCount = 0
            };

            _tourDateService.Add(tourDate);
            await _unitOfWork.SaveChangesAsync();

            return tourDate;
        }

        return null;
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

    private static DateTime CalculateEndDate(DateTime start, int durationValue, int durationUnitId)
    {
        return durationUnitId switch
        {
            DurationUnits.Ids.Hour => start.AddHours(durationValue),
            DurationUnits.Ids.Week => start.AddDays(durationValue * 7),
            DurationUnits.Ids.Month => start.AddMonths(durationValue),
            _ => start.AddDays(durationValue) // Day (default)
        };
    }

    private class MaterializedDateInfo
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Price { get; set; }
        public int? MaxCapacity { get; set; }
        public int BookedCount { get; set; }
        public bool IsAvailable { get; set; }
        public int? ScheduleId { get; set; }
        public bool IsCancelled { get; set; }
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
