using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Companies;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Companies;

public class CompanyProfileFactory : ICompanyProfileFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly ITourEntityService _tourService;
    private readonly IReviewEntityService _reviewService;

    public CompanyProfileFactory(
        ICompanyEntityService companyService,
        ITourEntityService tourService,
        IReviewEntityService reviewService)
    {
        _companyService = companyService;
        _tourService = tourService;
        _reviewService = reviewService;
    }

    public async Task<(bool found, object? result)> GetCompanyProfileAsync(string slug)
    {
        var company = await _companyService.GetBySlugAsync(slug);
        if (company == null || company.StatusId != CompanyStatuses.Ids.Approved)
            return (false, null);

        // Firmanin turlari
        var tours = await _tourService.GetActiveTours()
            .Where(t => t.CompanyId == company.Id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Destination,
                t.Price,
                t.DurationDays,
                t.MaxCapacity,
                t.ImageUrl,
                t.IsFeatured,
                t.DifficultyId,
                t.CategoryId,
                t.Latitude,
                t.Longitude,
                t.MeetingPointAddress,
                t.AverageRating,
                t.ReviewCount
            })
            .ToListAsync();

        // Firmanin toplam yorum istatistikleri
        var reviewStats = await _reviewService.GetApprovedReviews()
            .Where(r => r.Tour.CompanyId == company.Id)
            .GroupBy(r => 1)
            .Select(g => new
            {
                TotalReviews = g.Count(),
                AverageRating = g.Average(r => r.OverallRating),
                FiveStarCount = g.Count(r => r.OverallRating == 5),
                FourStarCount = g.Count(r => r.OverallRating == 4),
                ThreeStarCount = g.Count(r => r.OverallRating == 3),
                TwoStarCount = g.Count(r => r.OverallRating == 2),
                OneStarCount = g.Count(r => r.OverallRating == 1)
            })
            .FirstOrDefaultAsync();

        // Son 5 yorum
        var recentReviews = await _reviewService.GetApprovedReviews()
            .Include(r => r.Visitor)
            .Include(r => r.Tour)
            .Where(r => r.Tour.CompanyId == company.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                r.OverallRating,
                r.Title,
                r.Comment,
                r.Pros,
                r.Cons,
                r.IsVerified,
                r.WouldRecommend,
                r.CreatedAt,
                TourName = r.Tour.Name,
                VisitorName = r.Visitor.FirstName + " " + (r.Visitor.LastName.Length > 0 ? r.Visitor.LastName[0] + "." : "")
            })
            .ToListAsync();

        var result = new
        {
            company = new
            {
                company.Id,
                company.Name,
                company.Slug,
                company.Description,
                company.Tagline,
                company.Email,
                company.Phone,
                company.Address,
                company.City,
                company.Website,
                company.LogoUrl,
                company.CoverImageUrl,
                company.FoundedYear,
                company.SocialLinks,
                company.MetaTitle,
                company.MetaDescription
            },
            tours,
            tourCount = tours.Count,
            stats = reviewStats != null ? new
            {
                totalReviews = reviewStats.TotalReviews,
                averageRating = Math.Round(reviewStats.AverageRating, 1),
                ratingDistribution = new
                {
                    fiveStar = reviewStats.FiveStarCount,
                    fourStar = reviewStats.FourStarCount,
                    threeStar = reviewStats.ThreeStarCount,
                    twoStar = reviewStats.TwoStarCount,
                    oneStar = reviewStats.OneStarCount
                }
            } : new { totalReviews = 0, averageRating = 0.0, ratingDistribution = new { fiveStar = 0, fourStar = 0, threeStar = 0, twoStar = 0, oneStar = 0 } },
            recentReviews
        };

        return (true, result);
    }

    public async Task<object> GetPublicCompaniesAsync(string? city, string? search, string? sort)
    {
        var query = _companyService.GetActiveApprovedCompanies();

        // Sehir filtresi
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(c => c.City.ToLower().Contains(city.ToLower()));
        }

        // Arama
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.Description.ToLower().Contains(searchLower) ||
                c.City.ToLower().Contains(searchLower) ||
                c.Tagline.ToLower().Contains(searchLower));
        }

        var approvedReviews = _reviewService.GetApprovedReviews();

        // Her firma icin rating bilgisi hesapla
        var companiesWithRating = await query
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Tagline,
                c.City,
                c.LogoUrl,
                c.CoverImageUrl,
                c.FoundedYear,
                TourCount = c.Tours.Count(t => t.IsActive),
                ReviewCount = approvedReviews
                    .Where(r => r.Tour.CompanyId == c.Id)
                    .Count(),
                AverageRating = approvedReviews
                    .Where(r => r.Tour.CompanyId == c.Id)
                    .Any()
                    ? Math.Round(approvedReviews
                        .Where(r => r.Tour.CompanyId == c.Id)
                        .Average(r => r.OverallRating), 1)
                    : 0.0
            })
            .ToListAsync();

        // Siralama
        var sortedCompanies = sort switch
        {
            "name_asc" => companiesWithRating.OrderBy(c => c.Name).ToList(),
            "name_desc" => companiesWithRating.OrderByDescending(c => c.Name).ToList(),
            "rating" => companiesWithRating.OrderByDescending(c => c.AverageRating).ThenByDescending(c => c.ReviewCount).ToList(),
            "tours" => companiesWithRating.OrderByDescending(c => c.TourCount).ToList(),
            "newest" => companiesWithRating.OrderByDescending(c => c.Id).ToList(),
            _ => companiesWithRating.OrderBy(c => c.Name).ToList()
        };

        // Sehir listesi (filtreleme icin)
        var cities = await _companyService.GetActiveApprovedCompanies()
            .Where(c => !string.IsNullOrEmpty(c.City))
            .Select(c => c.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return new { companies = sortedCompanies, cities, total = sortedCompanies.Count };
    }

    public async Task<IEnumerable<Tour>> GetCompanyToursAsync(int companyId)
    {
        return await _tourService.GetByCompanyIdAsync(companyId);
    }
}
