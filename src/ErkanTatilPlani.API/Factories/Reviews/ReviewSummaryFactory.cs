using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Reviews;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Reviews;

/// <summary>
/// AI Yorum Ozeti - yorumlardan ortak temalari cikarir (Faz 15.2)
/// Yorumlardaki pros/cons/comment alanlarindan istatistiksel analiz yapar
/// </summary>
public class ReviewSummaryFactory : IReviewSummaryFactory
{
    private readonly IReviewEntityService _reviewService;
    private readonly ITourEntityService _tourService;

    public ReviewSummaryFactory(
        IReviewEntityService reviewService,
        ITourEntityService tourService)
    {
        _reviewService = reviewService;
        _tourService = tourService;
    }

    public async Task<object> GetTourReviewSummaryAsync(int tourId)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null)
            return new { message = "Tur bulunamadi" };

        var reviews = await _reviewService.GetApprovedReviews()
            .Where(r => r.TourId == tourId)
            .ToListAsync();

        if (reviews.Count == 0)
            return new
            {
                tourId,
                tourName = tour.Name,
                totalReviews = 0,
                message = "Henuz yorum yapilmamis"
            };

        // Ortalama puanlar
        var avgOverall = reviews.Average(r => r.OverallRating);
        var avgService = reviews.Where(r => r.ServiceRating.HasValue).Select(r => r.ServiceRating!.Value).DefaultIfEmpty(0).Average();
        var avgValue = reviews.Where(r => r.ValueRating.HasValue).Select(r => r.ValueRating!.Value).DefaultIfEmpty(0).Average();
        var avgLocation = reviews.Where(r => r.LocationRating.HasValue).Select(r => r.LocationRating!.Value).DefaultIfEmpty(0).Average();
        var avgOrganization = reviews.Where(r => r.OrganizationRating.HasValue).Select(r => r.OrganizationRating!.Value).DefaultIfEmpty(0).Average();
        var avgGuide = reviews.Where(r => r.GuideRating.HasValue).Select(r => r.GuideRating!.Value).DefaultIfEmpty(0).Average();

        // Puan dagilimi
        var ratingDistribution = Enumerable.Range(1, 5).Select(star => new
        {
            star,
            count = reviews.Count(r => r.OverallRating == star),
            percent = Math.Round(reviews.Count(r => r.OverallRating == star) * 100.0 / reviews.Count, 1)
        }).OrderByDescending(x => x.star).ToList();

        // Tavsiye orani
        var recommendPercent = Math.Round(reviews.Count(r => r.WouldRecommend) * 100.0 / reviews.Count, 1);

        // Seyahat tipi dagilimi
        var travelTypeDistribution = reviews.GroupBy(r => r.TravelTypeId)
            .Select(g => new
            {
                travelTypeId = g.Key,
                travelTypeName = TravelTypes.GetById(g.Key)?.SystemName ?? "Unknown",
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .ToList();

        // Kelime frekans analizi - Pros
        var prosKeywords = ExtractKeywords(reviews.Select(r => r.Pros));
        // Kelime frekans analizi - Cons
        var consKeywords = ExtractKeywords(reviews.Select(r => r.Cons));

        // En cok begenilenler
        var topPros = prosKeywords.Take(10).Select(k => new { keyword = k.Key, count = k.Value }).ToList();
        var topCons = consKeywords.Take(10).Select(k => new { keyword = k.Key, count = k.Value }).ToList();

        // Zaman trendi (son 6 ay)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var recentTrend = reviews
            .Where(r => r.CreatedAt >= sixMonthsAgo)
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new
            {
                period = $"{g.Key.Year}-{g.Key.Month:D2}",
                avgRating = Math.Round(g.Average(r => r.OverallRating), 1),
                count = g.Count()
            })
            .OrderBy(x => x.period)
            .ToList();

        // Dogrulanmis vs dogrulanmamis
        var verifiedCount = reviews.Count(r => r.IsVerified);
        var verifiedPercent = Math.Round(verifiedCount * 100.0 / reviews.Count, 1);

        return new
        {
            tourId,
            tourName = tour.Name,
            totalReviews = reviews.Count,
            averageRatings = new
            {
                overall = Math.Round(avgOverall, 1),
                service = Math.Round(avgService, 1),
                value = Math.Round(avgValue, 1),
                location = Math.Round(avgLocation, 1),
                organization = Math.Round(avgOrganization, 1),
                guide = Math.Round(avgGuide, 1)
            },
            ratingDistribution,
            recommendPercent,
            travelTypeDistribution,
            topPros,
            topCons,
            recentTrend,
            verifiedReviews = new { count = verifiedCount, percent = verifiedPercent }
        };
    }

    /// <summary>
    /// Basit kelime frekans analizi (stopword filtreleme ile)
    /// </summary>
    private static Dictionary<string, int> ExtractKeywords(IEnumerable<string> texts)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ve", "ile", "bir", "bu", "da", "de", "den", "dan", "icin", "olan", "olarak",
            "cok", "ama", "fakat", "ancak", "gibi", "kadar", "daha", "en", "the", "and",
            "is", "was", "were", "are", "been", "be", "to", "of", "in", "for", "on", "at",
            "by", "with", "it", "that", "this", "but", "not", "or", "an", "as", "very",
            "from", "we", "they", "our", "my", "her", "his", "so", "if", "has", "had",
            "var", "yok", "ise", "hem", "ya", "ne", "ki", "mi", "mu"
        };

        var frequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text)) continue;

            var words = text.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '-', '\n', '\r', '\t', '(', ')' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 3 && !stopWords.Contains(w));

            foreach (var word in words)
            {
                if (frequency.ContainsKey(word))
                    frequency[word]++;
                else
                    frequency[word] = 1;
            }
        }

        return frequency.OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
