using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.AbandonedCarts;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.AbandonedCarts;

public class AbandonedCartFactory : IAbandonedCartFactory
{
    private readonly IAbandonedCartEntityService _cartService;
    private readonly ITourEntityService _tourService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public AbandonedCartFactory(
        IAbandonedCartEntityService cartService,
        ITourEntityService tourService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _cartService = cartService;
        _tourService = tourService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task TrackCartAsync(int? visitorId, string email, int tourId, int? scheduleId, int numberOfPeople, decimal price, string? dateToken)
    {
        if (string.IsNullOrWhiteSpace(email)) return;

        // Ayni tur icin mevcut kayit varsa guncelle
        AbandonedCart? existing = null;
        if (visitorId.HasValue)
            existing = await _cartService.GetByVisitorAndTourAsync(visitorId.Value, tourId);
        existing ??= await _cartService.GetByEmailAndTourAsync(email, tourId);

        if (existing != null)
        {
            existing.NumberOfPeople = numberOfPeople;
            existing.Price = price;
            existing.ScheduleId = scheduleId;
            existing.DateToken = dateToken;
            existing.UpdatedAt = DateTime.UtcNow;
            _cartService.Update(existing);
        }
        else
        {
            _cartService.Add(new AbandonedCart
            {
                VisitorId = visitorId,
                Email = email,
                TourId = tourId,
                ScheduleId = scheduleId,
                NumberOfPeople = numberOfPeople,
                Price = price,
                DateToken = dateToken
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkRecoveredAsync(int visitorId, int tourId, int reservationId)
    {
        var cart = await _cartService.GetByVisitorAndTourAsync(visitorId, tourId);
        if (cart == null) return;

        cart.Recovered = true;
        cart.ReservationId = reservationId;
        cart.UpdatedAt = DateTime.UtcNow;
        _cartService.Update(cart);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ProcessAbandonedCartsAsync()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var carts = await _cartService.GetUnrecoveredCarts(oneHourAgo)
            .Include(c => c.Tour)
                .ThenInclude(t => t.Company)
            .Include(c => c.Visitor)
            .Take(50)
            .ToListAsync();

        foreach (var cart in carts)
        {
            try
            {
                var templateData = new Dictionary<string, string>
                {
                    ["email"] = cart.Email,
                    ["tourName"] = cart.Tour.Name,
                    ["tourDestination"] = cart.Tour.Destination,
                    ["companyName"] = cart.Tour.Company.Name,
                    ["numberOfPeople"] = cart.NumberOfPeople.ToString(),
                    ["price"] = cart.Price.ToString("F2"),
                    ["tourId"] = cart.TourId.ToString()
                };

                if (cart.Visitor != null)
                    templateData["visitorName"] = $"{cart.Visitor.FirstName} {cart.Visitor.LastName}";

                var language = cart.Visitor?.PreferredLanguage ?? "tr";
                await _emailService.SendTemplateEmailAsync(cart.Email, "abandoned_cart", language, templateData);

                cart.EmailSent = true;
                cart.EmailSentAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;
                _cartService.Update(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Abandoned cart email error: {ex.Message}");
            }
        }

        if (carts.Count > 0)
            await _unitOfWork.SaveChangesAsync();
    }
}
