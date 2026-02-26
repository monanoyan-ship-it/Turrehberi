using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Recommendations;

public class TourRecommendationFactory : ITourRecommendationFactory
{
    private readonly IReservationEntityService _reservationService;
    private readonly ITourEntityService _tourService;

    public TourRecommendationFactory(
        IReservationEntityService reservationService,
        ITourEntityService tourService)
    {
        _reservationService = reservationService;
        _tourService = tourService;
    }

    public async Task<object> GetRecommendationsAsync(int visitorId, int limit = 5)
    {
        var pastReservations = await _reservationService.GetByVisitorId(visitorId)
            .Include(r => r.Tour)
            .Where(r => r.Status == ReservationStatuses.Ids.Completed || r.Status == ReservationStatuses.Ids.Confirmed)
            .ToListAsync();

        if (pastReservations.Count == 0)
        {
            // Yeni kullanici: populer turlar oner
            var popularTours = await _tourService.GetActiveToursWithCompany()
                .Where(t => t.IsFeatured)
                .Take(limit)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Destination,
                    t.Price,
                    t.ImageUrl,
                    companyName = t.Company.Name,
                    t.AverageRating,
                    t.ReviewCount,
                    reason = "Popular"
                })
                .ToListAsync();
            return popularTours;
        }

        var visitedTourIds = pastReservations.Select(r => r.TourId).Distinct().ToList();
        var destinations = pastReservations.Select(r => r.Tour.Destination).Distinct().ToList();
        var categories = pastReservations.Select(r => r.Tour.CategoryId).Distinct().ToList();

        var recommendations = await _tourService.GetActiveToursWithCompany()
            .Where(t => !visitedTourIds.Contains(t.Id) &&
                        (destinations.Contains(t.Destination) || categories.Contains(t.CategoryId)))
            .OrderByDescending(t => t.AverageRating)
            .Take(limit)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Destination,
                t.Price,
                t.ImageUrl,
                companyName = t.Company.Name,
                t.AverageRating,
                t.ReviewCount,
                reason = destinations.Contains(t.Destination) ? "SameDestination" : "SameCategory"
            })
            .ToListAsync();

        return recommendations;
    }

    public async Task<object> GetRecommendationsByReservationAsync(int reservationId, int limit = 5)
    {
        var reservation = await _reservationService.GetByIdWithDetailsAsync(reservationId);
        if (reservation == null)
            return new List<object>();

        var tour = reservation.Tour;
        var recommendations = await _tourService.GetActiveToursWithCompany()
            .Where(t => t.Id != tour.Id &&
                        (t.Destination == tour.Destination || t.CategoryId == tour.CategoryId))
            .OrderByDescending(t => t.AverageRating)
            .Take(limit)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Destination,
                t.Price,
                t.ImageUrl,
                companyName = t.Company.Name,
                t.AverageRating,
                t.ReviewCount,
                reason = t.Destination == tour.Destination ? "SameDestination" : "SameCategory"
            })
            .ToListAsync();

        return recommendations;
    }
}
