using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Loyalty;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Loyalty;

public class LoyaltyFactory : ILoyaltyFactory
{
    private readonly ILoyaltyEntityService _loyaltyService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;

    public LoyaltyFactory(
        ILoyaltyEntityService loyaltyService,
        IVisitorEntityService visitorService,
        IReservationEntityService reservationService,
        IUnitOfWork unitOfWork)
    {
        _loyaltyService = loyaltyService;
        _visitorService = visitorService;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetLoyaltyDashboardAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null)
            return new { error = "Error.VisitorNotFound" };

        var tier = LoyaltyTiers.GetById(visitor.LoyaltyTierId);
        var completedTours = await _reservationService.GetByVisitorId(visitorId)
            .CountAsync(r => r.Status == ReservationStatuses.Ids.Completed);

        // Sonraki kademe icin ilerleme
        int? nextTierId = visitor.LoyaltyTierId < LoyaltyTiers.Ids.WorldTraveler
            ? visitor.LoyaltyTierId + 1
            : null;
        var nextTier = nextTierId.HasValue ? LoyaltyTiers.GetById(nextTierId.Value) : null;
        var nextTierMinTours = nextTierId.HasValue ? LoyaltyTiers.GetMinCompletedTours(nextTierId.Value) : 0;
        var progressPercent = nextTierMinTours > 0
            ? Math.Min(100, (int)((double)completedTours / nextTierMinTours * 100))
            : 100;

        return new
        {
            tierId = visitor.LoyaltyTierId,
            tierName = tier?.SystemName,
            tierNameKey = tier?.NameResourceKey,
            tierIcon = tier?.Icon,
            tierCssClass = tier?.CssClass,
            discountPercent = LoyaltyTiers.GetDiscountPercent(visitor.LoyaltyTierId),
            creditEarnPercent = LoyaltyTiers.GetCreditEarnPercent(visitor.LoyaltyTierId),
            creditBalance = visitor.CreditBalance,
            completedTours,
            nextTierNameKey = nextTier?.NameResourceKey,
            nextTierMinTours,
            progressPercent
        };
    }

    public async Task RecalculateTierAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return;

        var completedTours = await _reservationService.GetByVisitorId(visitorId)
            .CountAsync(r => r.Status == ReservationStatuses.Ids.Completed);

        int newTierId;
        if (completedTours >= LoyaltyTiers.GetMinCompletedTours(LoyaltyTiers.Ids.WorldTraveler))
            newTierId = LoyaltyTiers.Ids.WorldTraveler;
        else if (completedTours >= LoyaltyTiers.GetMinCompletedTours(LoyaltyTiers.Ids.Traveler))
            newTierId = LoyaltyTiers.Ids.Traveler;
        else
            newTierId = LoyaltyTiers.Ids.Explorer;

        if (newTierId != visitor.LoyaltyTierId)
        {
            var previousTierId = visitor.LoyaltyTierId;
            visitor.LoyaltyTierId = newTierId;
            visitor.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(visitor);

            _loyaltyService.AddTierHistory(new LoyaltyTierHistory
            {
                VisitorId = visitorId,
                PreviousTierId = previousTierId,
                NewTierId = newTierId,
                CompletedTourCount = completedTours,
                Reason = $"Completed tours: {completedTours}"
            });

            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task EarnCreditsAsync(int visitorId, int reservationId, decimal totalPrice)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return;

        var earnPercent = LoyaltyTiers.GetCreditEarnPercent(visitor.LoyaltyTierId);
        var creditAmount = Math.Round(totalPrice * earnPercent / 100, 2);
        if (creditAmount <= 0) return;

        visitor.CreditBalance += creditAmount;
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);

        _loyaltyService.AddCredit(new TourCredit
        {
            VisitorId = visitorId,
            TransactionTypeId = CreditTransactionTypes.Ids.Earn,
            Amount = creditAmount,
            BalanceAfter = visitor.CreditBalance,
            ReservationId = reservationId,
            Description = $"Credit earned from reservation #{reservationId}",
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(bool success, string? errorMessage)> SpendCreditsAsync(int visitorId, decimal amount, int reservationId)
    {
        if (amount <= 0)
            return (false, "Error.InvalidCreditAmount");

        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null)
            return (false, "Error.VisitorNotFound");

        if (visitor.CreditBalance < amount)
            return (false, "Error.InsufficientCredits");

        visitor.CreditBalance -= amount;
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);

        _loyaltyService.AddCredit(new TourCredit
        {
            VisitorId = visitorId,
            TransactionTypeId = CreditTransactionTypes.Ids.Spend,
            Amount = -amount,
            BalanceAfter = visitor.CreditBalance,
            ReservationId = reservationId,
            Description = $"Credit spent on reservation #{reservationId}"
        });

        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task ExpireCreditsAsync()
    {
        var expiredCredits = await _loyaltyService.GetExpirableCredits(DateTime.UtcNow).ToListAsync();

        foreach (var credit in expiredCredits)
        {
            var visitor = await _visitorService.GetByIdAsync(credit.VisitorId);
            if (visitor == null) continue;

            credit.IsActive = false;
            credit.UpdatedAt = DateTime.UtcNow;

            _loyaltyService.AddCredit(new TourCredit
            {
                VisitorId = credit.VisitorId,
                TransactionTypeId = CreditTransactionTypes.Ids.Expire,
                Amount = -credit.Amount,
                BalanceAfter = visitor.CreditBalance - credit.Amount,
                Description = $"Credit expired (originally earned {credit.CreatedAt:yyyy-MM-dd})"
            });

            visitor.CreditBalance -= credit.Amount;
            if (visitor.CreditBalance < 0) visitor.CreditBalance = 0;
            visitor.UpdatedAt = DateTime.UtcNow;
            _visitorService.Update(visitor);
        }

        if (expiredCredits.Count > 0)
            await _unitOfWork.SaveChangesAsync();
    }

    public async Task<object> GetCreditHistoryAsync(int visitorId, int page = 1, int pageSize = 20)
    {
        var query = _loyaltyService.GetByVisitorId(visitorId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                transactionType = CreditTransactionTypes.GetById(c.TransactionTypeId)!.SystemName,
                transactionTypeNameKey = CreditTransactionTypes.GetById(c.TransactionTypeId)!.NameResourceKey,
                c.Amount,
                c.BalanceAfter,
                c.ReservationId,
                c.Description,
                c.ExpiresAt,
                c.CreatedAt
            })
            .ToListAsync();

        return new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}
