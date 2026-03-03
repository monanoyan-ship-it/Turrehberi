using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Guides;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Guides;

/// <summary>
/// Bagimsiz rehber pazaryeri - herkese acik rehber katalogu (Faz 15.3)
/// </summary>
public class GuideMarketplaceFactory : IGuideMarketplaceFactory
{
    private readonly IGuideEntityService _guideService;
    private readonly ITourEntityService _tourService;

    public GuideMarketplaceFactory(
        IGuideEntityService guideService,
        ITourEntityService tourService)
    {
        _guideService = guideService;
        _tourService = tourService;
    }

    public async Task<object> GetPublicGuidesAsync(string? language = null, string? destination = null, int page = 1, int pageSize = 20)
    {
        var query = _guideService.GetActiveGuides()
            .Include(g => g.Company)
            .Include(g => g.Assignments)
                .ThenInclude(a => a.Schedule)
                    .ThenInclude(s => s.Tour)
            .AsQueryable();

        // Dil filtresi
        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(g => g.Languages.Contains(language));
        }

        // Destinasyon filtresi (rehberin atandigi turlarin destinasyonu)
        if (!string.IsNullOrEmpty(destination))
        {
            query = query.Where(g => g.Assignments.Any(a =>
                a.Schedule.Tour.Destination.Contains(destination)));
        }

        var totalCount = await query.CountAsync();
        var guides = await query
            .OrderByDescending(g => g.AverageRating)
            .ThenByDescending(g => g.TotalToursCompleted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new
            {
                g.Id,
                fullName = g.FirstName + " " + g.LastName,
                g.PhotoUrl,
                g.Languages,
                g.Bio,
                g.ExperienceYears,
                g.TotalToursCompleted,
                g.AverageRating,
                companyName = g.Company.Name,
                companyId = g.CompanyId,
                destinations = g.Assignments
                    .Where(a => a.Schedule != null && a.Schedule.Tour != null)
                    .Select(a => a.Schedule.Tour.Destination)
                    .Distinct()
                    .ToList()
            })
            .ToListAsync();

        return new
        {
            items = guides,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object> GetGuideProfileAsync(int guideId)
    {
        var guide = await _guideService.GetActiveGuides()
            .Include(g => g.Company)
            .Include(g => g.Assignments)
                .ThenInclude(a => a.Schedule)
                    .ThenInclude(s => s.Tour)
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            return new { message = "Rehber bulunamadi" };

        var upcomingTours = guide.Assignments
            .Where(a => a.Schedule != null && a.Schedule.ValidFrom >= DateTime.UtcNow)
            .Select(a => new
            {
                tourId = a.Schedule.TourId,
                tourName = a.Schedule.Tour?.Name,
                destination = a.Schedule.Tour?.Destination,
                validFrom = a.Schedule.ValidFrom,
                price = a.Schedule.Tour?.Price
            })
            .OrderBy(t => t.validFrom)
            .Take(10)
            .ToList();

        return new
        {
            guide.Id,
            fullName = guide.FirstName + " " + guide.LastName,
            guide.PhotoUrl,
            guide.Languages,
            guide.Bio,
            guide.ExperienceYears,
            guide.TotalToursCompleted,
            guide.AverageRating,
            companyName = guide.Company.Name,
            companyId = guide.CompanyId,
            upcomingTours,
            destinations = guide.Assignments
                .Where(a => a.Schedule?.Tour != null)
                .Select(a => a.Schedule.Tour!.Destination)
                .Distinct()
                .ToList()
        };
    }
}
